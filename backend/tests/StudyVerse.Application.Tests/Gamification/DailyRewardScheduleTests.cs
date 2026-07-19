using FluentAssertions;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Gamification;

public sealed class DailyRewardScheduleTests
{
    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 15, 0)]
    [InlineData(3, 20, 5)]
    [InlineData(4, 25, 5)]
    [InlineData(5, 30, 10)]
    [InlineData(6, 40, 10)]
    [InlineData(7, 50, 20)]
    public void GetReward_ForEachDayInTheCycle_ReturnsTheDocumentedEscalatingReward(int day, int expectedCoins, int expectedXp)
    {
        var (coins, xp) = DailyRewardSchedule.GetReward(day);

        coins.Should().Be(expectedCoins);
        xp.Should().Be(expectedXp);
    }

    [Fact]
    public void GetReward_RewardsStrictlyEscalateAcrossTheWholeCycle()
    {
        for (var day = 2; day <= DailyRewardSchedule.CycleLength; day++)
        {
            var (previousCoins, _) = DailyRewardSchedule.GetReward(day - 1);
            var (currentCoins, _) = DailyRewardSchedule.GetReward(day);

            currentCoins.Should().BeGreaterThan(previousCoins);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void GetReward_OutOfRangeDayNumber_Throws(int invalidDay)
    {
        var act = () => DailyRewardSchedule.GetReward(invalidDay);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NextDayNumber_BeforeTheEndOfTheCycle_JustIncrements()
    {
        DailyRewardSchedule.NextDayNumber(1).Should().Be(2);
        DailyRewardSchedule.NextDayNumber(6).Should().Be(7);
    }

    [Fact]
    public void NextDayNumber_AtTheEndOfTheCycle_WrapsBackToOne()
    {
        DailyRewardSchedule.NextDayNumber(7).Should().Be(1);
    }
}
