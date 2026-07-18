using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.StudyPlanner.ArchiveStudyPlan;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.StudyPlanner.ArchiveStudyPlan;

public sealed class ArchiveStudyPlanCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 9, 0, 0, DateTimeKind.Utc) };

    private ArchiveStudyPlanCommandHandler CreateHandler() => new(_db);

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

    private StudyPlan SeedPlan(Guid userId, StudyPlanStatus status = StudyPlanStatus.Active)
    {
        var plan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExamDate = DateOnly.FromDateTime(_dateTimeProvider.UtcNow).AddDays(10),
            SubjectsJson = "[]",
            WeakTopicsJson = "[]",
            HoursPerDayMinutes = 60,
            Status = status,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.StudyPlans.Add(plan);
        _db.SaveChanges();
        return plan;
    }

    [Fact]
    public async Task Handle_ActivePlan_ArchivesIt()
    {
        var userId = SeedUser();
        var plan = SeedPlan(userId);

        var result = await CreateHandler().Handle(new ArchiveStudyPlanCommand(userId, plan.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await _db.StudyPlans.SingleAsync(p => p.Id == plan.Id)).Status.Should().Be(StudyPlanStatus.Archived);
    }

    [Fact]
    public async Task Handle_AlreadyArchivedPlan_IsANoOpNotAnError()
    {
        var userId = SeedUser();
        var plan = SeedPlan(userId, StudyPlanStatus.Archived);

        var result = await CreateHandler().Handle(new ArchiveStudyPlanCommand(userId, plan.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await _db.StudyPlans.SingleAsync(p => p.Id == plan.Id)).Status.Should().Be(StudyPlanStatus.Archived);
    }

    [Fact]
    public async Task Handle_PlanOwnedByAnotherUser_FailsWithNotFoundAndDoesNotArchiveIt()
    {
        var ownerId = SeedUser();
        var attackerId = SeedUser();
        var plan = SeedPlan(ownerId);

        var result = await CreateHandler().Handle(new ArchiveStudyPlanCommand(attackerId, plan.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        (await _db.StudyPlans.SingleAsync(p => p.Id == plan.Id)).Status.Should().Be(StudyPlanStatus.Active);
    }

    [Fact]
    public async Task Handle_PlanThatDoesNotExist_FailsWithNotFound()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new ArchiveStudyPlanCommand(userId, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
