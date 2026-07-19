using FluentAssertions;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Gamification;

public sealed class SpinWheelSelectorTests
{
    [Fact]
    public void SelectPrize_EveryRollInRange_ReturnsAConfiguredPrize()
    {
        for (var roll = 0; roll < SpinPrizeCatalog.TotalWeight; roll++)
        {
            var prize = SpinWheelSelector.SelectPrize(roll);
            SpinPrizeCatalog.All.Should().Contain(prize);
        }
    }

    [Fact]
    public void SelectPrize_EveryConfiguredPrizeIsReachableByAtLeastOneRoll()
    {
        var reachablePrizes = Enumerable.Range(0, SpinPrizeCatalog.TotalWeight)
            .Select(SpinWheelSelector.SelectPrize)
            .Distinct()
            .ToList();

        reachablePrizes.Should().BeEquivalentTo(SpinPrizeCatalog.All);
    }

    [Fact]
    public void SelectPrize_AcrossEveryPossibleRoll_EachPrizeIsHitExactlyItsConfiguredWeightNumberOfTimes()
    {
        var hitCounts = Enumerable.Range(0, SpinPrizeCatalog.TotalWeight)
            .Select(SpinWheelSelector.SelectPrize)
            .GroupBy(p => p.Label)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var prize in SpinPrizeCatalog.All)
        {
            hitCounts[prize.Label].Should().Be(prize.Weight);
        }
    }

    [Fact]
    public void SelectPrize_RollBelowZero_FallsBackToTheLastConfiguredPrizeRatherThanThrowing()
    {
        // Defensive fallback path only reachable on a caller-contract violation (roll out of range).
        var prize = SpinWheelSelector.SelectPrize(SpinPrizeCatalog.TotalWeight + 100);

        prize.Should().Be(SpinPrizeCatalog.All[^1]);
    }
}
