using FluentAssertions;
using StudyVerse.Application.Features.StudyPlanner.Common;
using StudyVerse.Application.Features.StudyPlanner.GetTodayTasks;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.StudyPlanner.GetTodayTasks;

public sealed class GetTodayTasksQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 9, 0, 0, DateTimeKind.Utc) };
    private DateOnly Today => DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

    private GetTodayTasksQueryHandler CreateHandler() =>
        new(_db, _dateTimeProvider, new MissedTaskRecoveryService(_db, _dateTimeProvider));

    private Guid SeedUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            DisplayName = "Student",
            AuthProvider = AuthProvider.Local,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Users.Add(user);
        _db.SaveChanges();
        return user.Id;
    }

    private StudyPlan SeedActivePlan(Guid userId, DateOnly examDate, int hoursPerDayMinutes = 120)
    {
        var plan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExamDate = examDate,
            SubjectsJson = "[]",
            WeakTopicsJson = "[]",
            HoursPerDayMinutes = hoursPerDayMinutes,
            Status = StudyPlanStatus.Active,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.StudyPlans.Add(plan);
        _db.SaveChanges();
        return plan;
    }

    private void SeedTask(Guid planId, DateOnly scheduledDate, StudyPlanTaskStatus status, string subject = "Math", int durationMinutes = 60)
    {
        _db.StudyPlanTasks.Add(new StudyPlanTask
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            ScheduledDateUtc = scheduledDate,
            Subject = subject,
            Topic = "Topic",
            DurationMinutes = durationMinutes,
            Status = status,
        });
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_NoActivePlan_FailsWithNotFound()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetTodayTasksQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyTodaysPendingAndCompletedTasksNotOtherDaysOrMissed()
    {
        var userId = SeedUser();
        var plan = SeedActivePlan(userId, Today.AddDays(10));

        SeedTask(plan.Id, Today, StudyPlanTaskStatus.Pending, subject: "TodayPending");
        SeedTask(plan.Id, Today, StudyPlanTaskStatus.Completed, subject: "TodayCompleted");
        SeedTask(plan.Id, Today.AddDays(1), StudyPlanTaskStatus.Pending, subject: "Tomorrow");

        var result = await CreateHandler().Handle(new GetTodayTasksQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(t => t.Subject).Should().BeEquivalentTo(["TodayPending", "TodayCompleted"]);
    }

    [Fact]
    public async Task Handle_AnOverdueTask_IsRecoveredAndTomorrowsReplacementIsNotShownInTodaysList()
    {
        var userId = SeedUser();
        var plan = SeedActivePlan(userId, Today.AddDays(10));
        SeedTask(plan.Id, Today.AddDays(-1), StudyPlanTaskStatus.Pending, subject: "Overdue");

        var result = await CreateHandler().Handle(new GetTodayTasksQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // The replacement landed on tomorrow (capacity is wide open), not today, so today's list is empty.
        result.Value.Should().BeEmpty();

        var replacement = _db.StudyPlanTasks.Single(t => t.Status == StudyPlanTaskStatus.Pending);
        replacement.ScheduledDateUtc.Should().Be(Today.AddDays(1));
    }

    [Fact]
    public async Task Handle_AnotherUsersActivePlan_IsNeverVisible()
    {
        var ownerId = SeedUser();
        var requesterId = SeedUser();
        var plan = SeedActivePlan(ownerId, Today.AddDays(10));
        SeedTask(plan.Id, Today, StudyPlanTaskStatus.Pending);

        var result = await CreateHandler().Handle(new GetTodayTasksQuery(requesterId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
