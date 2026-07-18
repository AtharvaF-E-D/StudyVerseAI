using FluentAssertions;
using StudyVerse.Application.Features.StudyPlanner.Common;
using StudyVerse.Application.Features.StudyPlanner.GetActivePlan;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.StudyPlanner.GetActivePlan;

public sealed class GetActivePlanQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 9, 0, 0, DateTimeKind.Utc) };
    private DateOnly Today => DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

    private GetActivePlanQueryHandler CreateHandler() =>
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

    private StudyPlan SeedActivePlan(Guid userId, DateOnly examDate, IReadOnlyList<string>? subjects = null, IReadOnlyList<string>? weakTopics = null, int hoursPerDayMinutes = 120)
    {
        var plan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExamDate = examDate,
            SubjectsJson = System.Text.Json.JsonSerializer.Serialize(subjects ?? ["Math"]),
            WeakTopicsJson = System.Text.Json.JsonSerializer.Serialize(weakTopics ?? []),
            HoursPerDayMinutes = hoursPerDayMinutes,
            Status = StudyPlanStatus.Active,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.StudyPlans.Add(plan);
        _db.SaveChanges();
        return plan;
    }

    private void SeedTask(Guid planId, DateOnly scheduledDate, StudyPlanTaskStatus status, int durationMinutes = 60)
    {
        _db.StudyPlanTasks.Add(new StudyPlanTask
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            ScheduledDateUtc = scheduledDate,
            Subject = "Math",
            Topic = "Algebra",
            DurationMinutes = durationMinutes,
            Status = status,
        });
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_NoActivePlanForTheUser_FailsWithNotFound()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetActivePlanQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ActivePlanWithMixedTaskStatuses_ComputesCorrectSummaryCounts()
    {
        var userId = SeedUser();
        var examDate = Today.AddDays(10);
        var plan = SeedActivePlan(userId, examDate, subjects: ["Math", "Physics"], weakTopics: ["Physics"], hoursPerDayMinutes: 90);

        SeedTask(plan.Id, Today, StudyPlanTaskStatus.Completed);
        SeedTask(plan.Id, Today, StudyPlanTaskStatus.Pending);
        SeedTask(plan.Id, Today.AddDays(1), StudyPlanTaskStatus.Pending);

        var result = await CreateHandler().Handle(new GetActivePlanQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PlanId.Should().Be(plan.Id);
        result.Value.ExamDate.Should().Be(examDate);
        result.Value.DaysRemaining.Should().Be(10);
        result.Value.Subjects.Should().BeEquivalentTo(["Math", "Physics"]);
        result.Value.WeakTopics.Should().BeEquivalentTo(["Physics"]);
        result.Value.HoursPerDayMinutes.Should().Be(90);
        result.Value.TotalTasks.Should().Be(3);
        result.Value.CompletedTasks.Should().Be(1);
        result.Value.MissedTasks.Should().Be(0);
        result.Value.ProgressPercent.Should().BeApproximately(33.3, 0.1);
    }

    [Fact]
    public async Task Handle_OnlyReturnsTheRequestingUsersOwnPlan_NeverAnotherUsersActivePlan()
    {
        var ownerId = SeedUser();
        var otherUserId = SeedUser();
        SeedActivePlan(otherUserId, Today.AddDays(5));

        var result = await CreateHandler().Handle(new GetActivePlanQuery(ownerId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_AnOverdueStillPendingTask_IsSelfHealedBeforeCountsAreComputed()
    {
        var userId = SeedUser();
        var plan = SeedActivePlan(userId, Today.AddDays(10));
        SeedTask(plan.Id, Today.AddDays(-2), StudyPlanTaskStatus.Pending);

        var result = await CreateHandler().Handle(new GetActivePlanQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.MissedTasks.Should().Be(1);
        result.Value.TotalTasks.Should().Be(2); // the original (now Missed) plus its replacement.
    }
}
