using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.StudyPlanner.Common;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.StudyPlanner.Common;

public sealed class MissedTaskRecoveryServiceTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 9, 0, 0, DateTimeKind.Utc) };
    private DateOnly Today => DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

    private MissedTaskRecoveryService CreateService() => new(_db, _dateTimeProvider);

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

    private StudyPlanTask SeedTask(
        Guid planId,
        DateOnly scheduledDate,
        int durationMinutes,
        StudyPlanTaskStatus status = StudyPlanTaskStatus.Pending,
        string subject = "Math",
        string topic = "Algebra",
        bool isWeakTopic = false)
    {
        var task = new StudyPlanTask
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            ScheduledDateUtc = scheduledDate,
            Subject = subject,
            Topic = topic,
            DurationMinutes = durationMinutes,
            IsWeakTopic = isWeakTopic,
            Status = status,
        };
        _db.StudyPlanTasks.Add(task);
        _db.SaveChanges();
        return task;
    }

    [Fact]
    public async Task RecoverAsync_TaskScheduledThreeDaysAgoStillPending_IsMarkedMissedAndAReplacementAppearsTomorrow()
    {
        var userId = SeedUser();
        var plan = SeedActivePlan(userId, Today.AddDays(30));
        var missed = SeedTask(plan.Id, Today.AddDays(-3), 60, subject: "Physics", topic: "Kinematics", isWeakTopic: true);

        await CreateService().RecoverAsync(userId, CancellationToken.None);

        var reloadedMissed = await _db.StudyPlanTasks.SingleAsync(t => t.Id == missed.Id);
        reloadedMissed.Status.Should().Be(StudyPlanTaskStatus.Missed);

        var replacement = await _db.StudyPlanTasks.SingleAsync(t => t.Id != missed.Id);
        replacement.Status.Should().Be(StudyPlanTaskStatus.Pending);
        replacement.ScheduledDateUtc.Should().Be(Today.AddDays(1));
        replacement.Subject.Should().Be("Physics");
        replacement.Topic.Should().Be("Kinematics");
        replacement.DurationMinutes.Should().Be(60);
        replacement.IsWeakTopic.Should().BeTrue();
        replacement.OriginalScheduledDateUtc.Should().Be(Today.AddDays(-3));
    }

    [Fact]
    public async Task RecoverAsync_EveryDayThroughTheExamIsAlreadyFull_OverflowsOntoTheDayBeforeTheExam()
    {
        var userId = SeedUser();
        var examDate = Today.AddDays(7);
        var plan = SeedActivePlan(userId, examDate, hoursPerDayMinutes: 60);

        // Fully book every future day (tomorrow through the exam date) with a 60-minute filler task,
        // exactly saturating the 60-minute daily budget.
        for (var day = Today.AddDays(1); day <= examDate; day = day.AddDays(1))
        {
            SeedTask(plan.Id, day, 60, subject: "Filler", topic: "Filler");
        }

        var missed = SeedTask(plan.Id, Today.AddDays(-1), 60, subject: "Chemistry", topic: "Stoichiometry");

        await CreateService().RecoverAsync(userId, CancellationToken.None);

        var replacement = await _db.StudyPlanTasks.SingleAsync(
            t => t.Subject == "Chemistry" && t.Status == StudyPlanTaskStatus.Pending);
        replacement.ScheduledDateUtc.Should().Be(examDate.AddDays(-1));
        replacement.OriginalScheduledDateUtc.Should().Be(missed.ScheduledDateUtc);
    }

    [Fact]
    public async Task RecoverAsync_AlreadyCompletedPastTasks_AreLeftUntouchedWithNoReplacementCreated()
    {
        var userId = SeedUser();
        var plan = SeedActivePlan(userId, Today.AddDays(30));
        var completed = SeedTask(plan.Id, Today.AddDays(-5), 45, status: StudyPlanTaskStatus.Completed);

        await CreateService().RecoverAsync(userId, CancellationToken.None);

        var reloaded = await _db.StudyPlanTasks.SingleAsync(t => t.Id == completed.Id);
        reloaded.Status.Should().Be(StudyPlanTaskStatus.Completed);

        (await _db.StudyPlanTasks.CountAsync(t => t.PlanId == plan.Id)).Should().Be(1);
    }

    [Fact]
    public async Task RecoverAsync_NoActivePlanForTheUser_IsANoOp()
    {
        var userId = SeedUser();

        await CreateService().RecoverAsync(userId, CancellationToken.None);

        (await _db.StudyPlanTasks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RecoverAsync_SeveralMissedTasksInOnePass_SpreadAcrossDifferentDaysRatherThanAllPilingOnTomorrow()
    {
        var userId = SeedUser();
        var plan = SeedActivePlan(userId, Today.AddDays(30), hoursPerDayMinutes: 60);

        var missedOne = SeedTask(plan.Id, Today.AddDays(-2), 60, subject: "A", topic: "A1");
        var missedTwo = SeedTask(plan.Id, Today.AddDays(-1), 60, subject: "B", topic: "B1");

        await CreateService().RecoverAsync(userId, CancellationToken.None);

        var replacementOne = await _db.StudyPlanTasks.SingleAsync(t => t.Subject == "A" && t.Status == StudyPlanTaskStatus.Pending);
        var replacementTwo = await _db.StudyPlanTasks.SingleAsync(t => t.Subject == "B" && t.Status == StudyPlanTaskStatus.Pending);

        replacementOne.ScheduledDateUtc.Should().Be(Today.AddDays(1));
        replacementTwo.ScheduledDateUtc.Should().Be(Today.AddDays(2));
    }
}
