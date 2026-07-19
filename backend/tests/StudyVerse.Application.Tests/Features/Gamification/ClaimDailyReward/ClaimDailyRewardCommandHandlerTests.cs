using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.Gamification;
using StudyVerse.Application.Features.Gamification.ClaimDailyReward;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Features.Gamification.ClaimDailyReward;

public sealed class ClaimDailyRewardCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private ClaimDailyRewardCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

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

    [Fact]
    public async Task Handle_FirstEverClaim_IsDayOneAndAwardsTheDayOneReward()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DayNumber.Should().Be(1);
        result.Value.CoinsAwarded.Should().Be(10);
        result.Value.XpAwarded.Should().Be(0);
        result.Value.NewCoinsTotal.Should().Be(10);
        result.Value.NewXpTotal.Should().Be(0);

        var claim = await _db.DailyRewardClaims.SingleAsync(c => c.UserId == userId);
        claim.ConsecutiveDayNumber.Should().Be(1);
        claim.CoinsAwarded.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ClaimingTwiceOnTheSameDay_RejectsTheSecondClaimWithConflict()
    {
        var userId = SeedUser();
        var handler = CreateHandler();

        var first = await handler.Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var second = await handler.Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);

        second.IsSuccess.Should().BeFalse();
        second.ErrorType.Should().Be(ResultErrorType.Conflict);

        (await _db.DailyRewardClaims.CountAsync(c => c.UserId == userId)).Should().Be(1);
        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.Coins.Should().Be(10); // only the first claim's coins.
    }

    [Fact]
    public async Task Handle_OnTheNextConsecutiveDay_AdvancesToDayTwoAndEscalatesTheReward()
    {
        var userId = SeedUser();
        var handler = CreateHandler();
        await handler.Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(1);
        var result = await handler.Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DayNumber.Should().Be(2);
        result.Value.CoinsAwarded.Should().Be(15);
        result.Value.NewCoinsTotal.Should().Be(10 + 15);
    }

    [Fact]
    public async Task Handle_AfterAGapOfTwoOrMoreDays_ResetsBackToDayOne()
    {
        var userId = SeedUser();
        var handler = CreateHandler();
        await handler.Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(1);
        await handler.Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);
        // Day number is now 2.

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(3); // a 2-day gap
        var result = await handler.Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DayNumber.Should().Be(1);
        result.Value.CoinsAwarded.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ClaimingSevenConsecutiveDays_ReachesTheDaySevenRewardThenWrapsBackToDayOne()
    {
        var userId = SeedUser();
        var handler = CreateHandler();

        ClaimDailyRewardResultDto? lastResult = null;
        for (var day = 1; day <= 7; day++)
        {
            var result = await handler.Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            lastResult = result.Value;
            _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(1);
        }

        lastResult!.DayNumber.Should().Be(7);
        lastResult.CoinsAwarded.Should().Be(50);
        lastResult.XpAwarded.Should().Be(20);

        // The 8th consecutive day wraps back to day 1.
        var eighthDayResult = await handler.Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);
        eighthDayResult.Value.DayNumber.Should().Be(1);
        eighthDayResult.Value.CoinsAwarded.Should().Be(10);
    }

    [Fact]
    public async Task Handle_DuringAnActiveSeasonalEvent_AddsTheEventBonusCoinsOnTopOfTheScheduledReward()
    {
        var userId = SeedUser();
        var activeEvent = SeasonalEventCatalog.All.Single();
        _dateTimeProvider.UtcNow = activeEvent.StartDateUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var result = await CreateHandler().Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SeasonalEventName.Should().Be(activeEvent.Name);
        result.Value.SeasonalEventBonusCoins.Should().Be(activeEvent.DailyRewardBonusCoins);
        result.Value.CoinsAwarded.Should().Be(10 + activeEvent.DailyRewardBonusCoins); // day 1 (10 coins) + bonus
    }
}
