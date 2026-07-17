using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.Dashboard.CompleteChallenge;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Features.Dashboard.CompleteChallenge;

public sealed class CompleteChallengeCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private CompleteChallengeCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

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

    private DateOnly Today => DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

    [Fact]
    public async Task Handle_WithOneOfTodaysChallenges_AwardsXpAndCoinsAndRecordsTheCompletion()
    {
        var userId = SeedUser();
        var template = DailyChallengeSelector.GetTodaysTemplates(Today)[0];

        var result = await CreateHandler().Handle(
            new CompleteChallengeCommand(userId, template.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.XpAwarded.Should().Be(template.XpReward);
        result.Value.CoinsAwarded.Should().Be(template.CoinReward);
        result.Value.NewXpTotal.Should().Be(template.XpReward);
        result.Value.NewCoinsTotal.Should().Be(template.CoinReward);
        result.Value.NewLevel.Should().Be(LevelCalculator.GetLevel(template.XpReward));

        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.Xp.Should().Be(template.XpReward);
        progress.Coins.Should().Be(template.CoinReward);

        var completion = await _db.ChallengeCompletions.SingleAsync(c => c.UserId == userId);
        completion.ChallengeTemplateId.Should().Be(template.Id);
        completion.CompletedDateUtc.Should().Be(Today);
        completion.XpAwarded.Should().Be(template.XpReward);
        completion.CoinsAwarded.Should().Be(template.CoinReward);
    }

    [Fact]
    public async Task Handle_TheSameChallengeTwiceOnTheSameDay_TheSecondAttemptFailsWithConflict()
    {
        var userId = SeedUser();
        var template = DailyChallengeSelector.GetTodaysTemplates(Today)[0];

        var first = await CreateHandler().Handle(
            new CompleteChallengeCommand(userId, template.Id),
            CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var second = await CreateHandler().Handle(
            new CompleteChallengeCommand(userId, template.Id),
            CancellationToken.None);

        second.IsSuccess.Should().BeFalse();
        second.ErrorType.Should().Be(ResultErrorType.Conflict);

        (await _db.ChallengeCompletions.CountAsync(c => c.UserId == userId)).Should().Be(1);

        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.Xp.Should().Be(template.XpReward);
    }

    [Fact]
    public async Task Handle_WithATemplateThatIsNotOneOfTodaysThree_FailsWithAValidationError()
    {
        var userId = SeedUser();
        var todaysIds = DailyChallengeSelector.GetTodaysTemplates(Today).Select(t => t.Id).ToHashSet();
        var notTodaysTemplateId = ChallengeCatalog.All.Select(t => t.Id).First(id => !todaysIds.Contains(id));

        var result = await CreateHandler().Handle(
            new CompleteChallengeCommand(userId, notTodaysTemplateId),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);

        (await _db.ChallengeCompletions.CountAsync()).Should().Be(0);
    }
}
