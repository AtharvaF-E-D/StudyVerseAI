using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.StudyPlanner.CreateStudyPlan;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.StudyPlanner.CreateStudyPlan;

public sealed class CreateStudyPlanCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 9, 0, 0, DateTimeKind.Utc) };
    private readonly IAiChatProvider _aiChatProvider = Substitute.For<IAiChatProvider>();
    private DateOnly Today => DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

    private CreateStudyPlanCommandHandler CreateHandler() => new(_db, _dateTimeProvider, _aiChatProvider);

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

    private void StubAiPlan(params (DateOnly Date, string Subject, string Topic, int DurationMinutes, bool IsWeakTopic)[] tasks)
    {
        var payload = new
        {
            tasks = tasks.Select(t => new
            {
                date = t.Date.ToString("yyyy-MM-dd"),
                subject = t.Subject,
                topic = t.Topic,
                durationMinutes = t.DurationMinutes,
                isWeakTopic = t.IsWeakTopic,
            }),
        };
        var json = JsonSerializer.Serialize(payload);

        _aiChatProvider
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(new AiChatResult(json, 100, 200));
    }

    [Fact]
    public async Task Handle_ValidRequestWithAGoodAiResponse_PersistsThePlanAndEveryGeneratedTask()
    {
        var userId = SeedUser();
        var examDate = Today.AddDays(14);
        StubAiPlan(
            (Today.AddDays(1), "Math", "Algebra", 60, false),
            (Today.AddDays(2), "Physics", "Kinematics", 90, true));

        var result = await CreateHandler().Handle(
            new CreateStudyPlanCommand(userId, examDate, ["Math", "Physics"], ["Physics"], 120),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExamDate.Should().Be(examDate);
        result.Value.TotalTasks.Should().Be(2);

        var plan = await _db.StudyPlans.SingleAsync(p => p.UserId == userId);
        plan.Status.Should().Be(StudyPlanStatus.Active);
        plan.HoursPerDayMinutes.Should().Be(120);

        var tasks = await _db.StudyPlanTasks.Where(t => t.PlanId == plan.Id).ToListAsync();
        tasks.Should().HaveCount(2);
        tasks.Should().OnlyContain(t => t.Status == StudyPlanTaskStatus.Pending);
        tasks.Single(t => t.Subject == "Physics").IsWeakTopic.Should().BeTrue();
        tasks.Single(t => t.Subject == "Math").IsWeakTopic.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ExamDateIsToday_FailsValidationWithoutCallingTheAi()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(
            new CreateStudyPlanCommand(userId, Today, ["Math"], [], 60),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
        await _aiChatProvider.DidNotReceiveWithAnyArgs().GetCompletionAsync(default!, default, default);
    }

    [Fact]
    public async Task Handle_ExamDateInThePast_FailsValidation()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(
            new CreateStudyPlanCommand(userId, Today.AddDays(-1), ["Math"], [], 60),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task Handle_AiReturnsNoUsableTasks_FailsWithoutPersistingAPlan()
    {
        var userId = SeedUser();
        StubAiPlan(); // empty "tasks" array

        var result = await CreateHandler().Handle(
            new CreateStudyPlanCommand(userId, Today.AddDays(10), ["Math"], [], 60),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        (await _db.StudyPlans.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Handle_UserAlreadyHasAnActivePlan_ArchivesItAndTheNewPlanBecomesTheOnlyActiveOne()
    {
        var userId = SeedUser();
        var priorPlan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExamDate = Today.AddDays(5),
            SubjectsJson = "[]",
            WeakTopicsJson = "[]",
            HoursPerDayMinutes = 60,
            Status = StudyPlanStatus.Active,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.StudyPlans.Add(priorPlan);
        _db.SaveChanges();

        StubAiPlan((Today.AddDays(1), "Math", "Algebra", 60, false));

        var result = await CreateHandler().Handle(
            new CreateStudyPlanCommand(userId, Today.AddDays(20), ["Math"], [], 90),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var reloadedPrior = await _db.StudyPlans.SingleAsync(p => p.Id == priorPlan.Id);
        reloadedPrior.Status.Should().Be(StudyPlanStatus.Archived);

        var activePlans = await _db.StudyPlans
            .Where(p => p.UserId == userId && p.Status == StudyPlanStatus.Active)
            .ToListAsync();
        activePlans.Should().ContainSingle(p => p.Id == result.Value.PlanId);
    }

    [Fact]
    public async Task Handle_AiReturnsSomeDatesOutsideThePlanWindow_OnlyTheInRangeTasksArePersisted()
    {
        var userId = SeedUser();
        var examDate = Today.AddDays(5);
        StubAiPlan(
            (Today.AddDays(1), "Math", "InRange", 60, false),
            (Today.AddDays(50), "Math", "WayOutOfRange", 60, false));

        var result = await CreateHandler().Handle(
            new CreateStudyPlanCommand(userId, examDate, ["Math"], [], 120),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalTasks.Should().Be(1);

        var persistedTask = await _db.StudyPlanTasks.SingleAsync();
        persistedTask.Topic.Should().Be("InRange");
    }
}
