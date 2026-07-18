using FluentAssertions;
using StudyVerse.Application.Features.StudyPlanner.GetWeeklyTasks;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.StudyPlanner.GetWeeklyTasks;

public sealed class GetWeeklyTasksQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();

    // 2026-07-18 is a Saturday - deliberately not a Monday, so the "most recent Monday" math is
    // actually exercised rather than trivially matching "today".
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 9, 0, 0, DateTimeKind.Utc) };

    private GetWeeklyTasksQueryHandler CreateHandler() => new(_db, _dateTimeProvider);

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

    private StudyPlan SeedActivePlan(Guid userId, DateOnly examDate)
    {
        var plan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExamDate = examDate,
            SubjectsJson = "[]",
            WeakTopicsJson = "[]",
            HoursPerDayMinutes = 120,
            Status = StudyPlanStatus.Active,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.StudyPlans.Add(plan);
        _db.SaveChanges();
        return plan;
    }

    private void SeedTask(Guid planId, DateOnly scheduledDate, string subject, StudyPlanTaskStatus status = StudyPlanTaskStatus.Pending)
    {
        _db.StudyPlanTasks.Add(new StudyPlanTask
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            ScheduledDateUtc = scheduledDate,
            Subject = subject,
            Topic = "Topic",
            DurationMinutes = 60,
            Status = status,
        });
        _db.SaveChanges();
    }

    [Fact]
    public void SanityCheck_TestClockIsASaturday()
    {
        DateOnly.FromDateTime(_dateTimeProvider.UtcNow).DayOfWeek.Should().Be(DayOfWeek.Saturday);
    }

    [Fact]
    public async Task Handle_NoWeekStartDateProvided_DefaultsToTheMostRecentMondayOnOrBeforeToday()
    {
        var userId = SeedUser();
        var plan = SeedActivePlan(userId, DateOnly.FromDateTime(_dateTimeProvider.UtcNow).AddDays(30));

        // 2026-07-18 is a Saturday, so the most recent Monday on/before it is 2026-07-13.
        var expectedMonday = new DateOnly(2026, 7, 13);
        SeedTask(plan.Id, expectedMonday, "InWeek");
        SeedTask(plan.Id, expectedMonday.AddDays(6), "AlsoInWeek"); // the Sunday closing the week
        SeedTask(plan.Id, expectedMonday.AddDays(-1), "BeforeWeek");
        SeedTask(plan.Id, expectedMonday.AddDays(7), "AfterWeek");

        var result = await CreateHandler().Handle(new GetWeeklyTasksQuery(userId, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(t => t.Subject).Should().BeEquivalentTo(["InWeek", "AlsoInWeek"]);
    }

    [Fact]
    public async Task Handle_ExplicitWeekStartDate_OverridesTheDefaultMondayMath()
    {
        var userId = SeedUser();
        var plan = SeedActivePlan(userId, DateOnly.FromDateTime(_dateTimeProvider.UtcNow).AddDays(30));

        var explicitStart = new DateOnly(2026, 8, 3);
        SeedTask(plan.Id, explicitStart, "InRequestedWeek");
        SeedTask(plan.Id, new DateOnly(2026, 7, 13), "DefaultWeekOnly");

        var result = await CreateHandler().Handle(new GetWeeklyTasksQuery(userId, explicitStart), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(t => t.Subject).Should().BeEquivalentTo(["InRequestedWeek"]);
    }

    [Fact]
    public async Task Handle_WeeklyViewIncludesMissedTasksToo_UnlikeTheTodayView()
    {
        var userId = SeedUser();
        var plan = SeedActivePlan(userId, DateOnly.FromDateTime(_dateTimeProvider.UtcNow).AddDays(30));
        var expectedMonday = new DateOnly(2026, 7, 13);
        SeedTask(plan.Id, expectedMonday.AddDays(2), "MissedOne", StudyPlanTaskStatus.Missed);

        var result = await CreateHandler().Handle(new GetWeeklyTasksQuery(userId, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(t => t.Subject == "MissedOne");
    }

    [Fact]
    public async Task Handle_NoActivePlan_FailsWithNotFound()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetWeeklyTasksQuery(userId, null), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
