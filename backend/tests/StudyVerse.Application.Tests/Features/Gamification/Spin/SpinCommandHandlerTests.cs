using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Gamification.Spin;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Features.Gamification.Spin;

public sealed class SpinCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();
    private readonly IRandomProvider _randomProvider = Substitute.For<IRandomProvider>();

    private SpinCommandHandler CreateHandler() => new(_db, _dateTimeProvider, _randomProvider);

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

    private void StubRoll(int roll) =>
        _randomProvider.Next(0, SpinPrizeCatalog.TotalWeight).Returns(roll);

    [Fact]
    public async Task Handle_RollLandsOnAKnownPrize_AwardsExactlyThatPrizesCoinsAndXp()
    {
        var userId = SeedUser();
        // Roll 0 always lands on the first prize in the catalog (cumulative-weight bucketing starts at 0).
        var firstPrize = SpinPrizeCatalog.All[0];
        StubRoll(0);

        var result = await CreateHandler().Handle(new SpinCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PrizeLabel.Should().Be(firstPrize.Label);
        result.Value.CoinsAwarded.Should().Be(firstPrize.CoinsAwarded);
        result.Value.XpAwarded.Should().Be(firstPrize.XpAwarded);
        result.Value.NewCoinsTotal.Should().Be(firstPrize.CoinsAwarded);
        result.Value.NewXpTotal.Should().Be(firstPrize.XpAwarded);

        var spin = await _db.SpinResults.SingleAsync(s => s.UserId == userId);
        spin.PrizeLabel.Should().Be(firstPrize.Label);
        spin.CoinsAwarded.Should().Be(firstPrize.CoinsAwarded);
        spin.SpinDateUtc.Should().Be(DateOnly.FromDateTime(_dateTimeProvider.UtcNow));
    }

    [Fact]
    public async Task Handle_RollLandsOnTheJackpot_AwardsTheJackpotReward()
    {
        var userId = SeedUser();
        var jackpot = SpinPrizeCatalog.All.Single(p => p.Label.Contains("Jackpot"));
        // The jackpot is the last (rarest) prize - its bucket ends exactly at TotalWeight - 1.
        StubRoll(SpinPrizeCatalog.TotalWeight - 1);

        var result = await CreateHandler().Handle(new SpinCommand(userId), CancellationToken.None);

        result.Value.PrizeLabel.Should().Be(jackpot.Label);
        result.Value.CoinsAwarded.Should().Be(jackpot.CoinsAwarded);
        result.Value.XpAwarded.Should().Be(jackpot.XpAwarded);
    }

    [Fact]
    public async Task Handle_SpinningTwiceOnTheSameDay_RejectsTheSecondSpinWithConflict()
    {
        var userId = SeedUser();
        StubRoll(0);
        var handler = CreateHandler();

        var first = await handler.Handle(new SpinCommand(userId), CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var second = await handler.Handle(new SpinCommand(userId), CancellationToken.None);

        second.IsSuccess.Should().BeFalse();
        second.ErrorType.Should().Be(ResultErrorType.Conflict);

        (await _db.SpinResults.CountAsync(s => s.UserId == userId)).Should().Be(1);
    }

    [Fact]
    public async Task Handle_OnANewDay_AllowsSpinningAgain()
    {
        var userId = SeedUser();
        StubRoll(0);
        var handler = CreateHandler();

        await handler.Handle(new SpinCommand(userId), CancellationToken.None);
        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(1);
        var result = await handler.Handle(new SpinCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await _db.SpinResults.CountAsync(s => s.UserId == userId)).Should().Be(2);
    }

    [Fact]
    public async Task Handle_TwoSpinsOnDifferentDays_AccumulatesCoinsAcrossBoth()
    {
        var userId = SeedUser();
        var handler = CreateHandler();

        StubRoll(0); // first prize
        await handler.Handle(new SpinCommand(userId), CancellationToken.None);

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(1);
        StubRoll(SpinPrizeCatalog.TotalWeight - 1); // jackpot
        await handler.Handle(new SpinCommand(userId), CancellationToken.None);

        var expectedCoins = SpinPrizeCatalog.All[0].CoinsAwarded + SpinPrizeCatalog.All[^1].CoinsAwarded;
        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.Coins.Should().Be(expectedCoins);
    }
}
