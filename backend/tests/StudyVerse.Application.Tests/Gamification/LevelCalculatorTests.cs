using FluentAssertions;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Gamification;

public sealed class LevelCalculatorTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(49, 1)]
    [InlineData(50, 2)]
    [InlineData(199, 2)]
    [InlineData(200, 3)]
    [InlineData(449, 3)]
    [InlineData(450, 4)]
    public void GetLevel_ReturnsExpectedLevelForKnownXpThresholds(int xp, int expectedLevel)
    {
        LevelCalculator.GetLevel(xp).Should().Be(expectedLevel);
    }
}
