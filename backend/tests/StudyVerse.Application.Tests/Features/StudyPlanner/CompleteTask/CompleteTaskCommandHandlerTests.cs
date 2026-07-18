using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.StudyPlanner.CompleteTask;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.StudyPlanner.CompleteTask;

public sealed class CompleteTaskCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 9, 0, 0, DateTimeKind.Utc) };

    private CompleteTaskCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

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

    private (StudyPlan Plan, StudyPlanTask Task) SeedPlanWithTask(Guid userId, StudyPlanTaskStatus status = StudyPlanTaskStatus.Pending, DateTime? completedAtUtc = null)
    {
        var plan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExamDate = DateOnly.FromDateTime(_dateTimeProvider.UtcNow).AddDays(10),
            SubjectsJson = "[]",
            WeakTopicsJson = "[]",
            HoursPerDayMinutes = 60,
            Status = StudyPlanStatus.Active,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.StudyPlans.Add(plan);

        var task = new StudyPlanTask
        {
            Id = Guid.NewGuid(),
            PlanId = plan.Id,
            ScheduledDateUtc = DateOnly.FromDateTime(_dateTimeProvider.UtcNow),
            Subject = "Math",
            Topic = "Algebra",
            DurationMinutes = 60,
            Status = status,
            CompletedAtUtc = completedAtUtc,
        };
        _db.StudyPlanTasks.Add(task);
        _db.SaveChanges();

        return (plan, task);
    }

    [Fact]
    public async Task Handle_PendingTask_MarksItCompletedAndStampsCompletedAtUtc()
    {
        var userId = SeedUser();
        var (_, task) = SeedPlanWithTask(userId);

        var result = await CreateHandler().Handle(new CompleteTaskCommand(userId, task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var reloaded = await _db.StudyPlanTasks.SingleAsync(t => t.Id == task.Id);
        reloaded.Status.Should().Be(StudyPlanTaskStatus.Completed);
        reloaded.CompletedAtUtc.Should().Be(_dateTimeProvider.UtcNow);
    }

    [Fact]
    public async Task Handle_AlreadyCompletedTask_IsANoOpNotAnErrorAndDoesNotOverwriteTheOriginalTimestamp()
    {
        var userId = SeedUser();
        var originalCompletionTime = new DateTime(2026, 7, 10, 8, 0, 0, DateTimeKind.Utc);
        var (_, task) = SeedPlanWithTask(userId, StudyPlanTaskStatus.Completed, originalCompletionTime);

        var result = await CreateHandler().Handle(new CompleteTaskCommand(userId, task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var reloaded = await _db.StudyPlanTasks.SingleAsync(t => t.Id == task.Id);
        reloaded.Status.Should().Be(StudyPlanTaskStatus.Completed);
        reloaded.CompletedAtUtc.Should().Be(originalCompletionTime);
    }

    [Fact]
    public async Task Handle_TaskOwnedByAnotherUser_FailsWithNotFound()
    {
        var ownerId = SeedUser();
        var attackerId = SeedUser();
        var (_, task) = SeedPlanWithTask(ownerId);

        var result = await CreateHandler().Handle(new CompleteTaskCommand(attackerId, task.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);

        var reloaded = await _db.StudyPlanTasks.SingleAsync(t => t.Id == task.Id);
        reloaded.Status.Should().Be(StudyPlanTaskStatus.Pending);
    }

    [Fact]
    public async Task Handle_TaskThatDoesNotExist_FailsWithNotFound()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new CompleteTaskCommand(userId, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
