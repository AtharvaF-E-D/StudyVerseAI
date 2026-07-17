using FluentAssertions;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Tests.Quiz;

public sealed class QuizScoringTests
{
    [Theory]
    [InlineData(0, 1.0)]
    [InlineData(1, 1.1)]
    [InlineData(2, 1.2)]
    [InlineData(3, 1.3)]
    [InlineData(4, 1.4)]
    [InlineData(5, 1.5)]
    [InlineData(6, 1.5)]
    [InlineData(100, 1.5)]
    public void GetComboMultiplier_ReturnsExpectedMultiplierCappedAt1Point5x(int comboCount, double expectedMultiplier)
    {
        QuizScoring.GetComboMultiplier(comboCount).Should().BeApproximately(expectedMultiplier, 0.0001);
    }

    [Theory]
    [InlineData(QuizDifficulty.Easy, 0, 10)]
    [InlineData(QuizDifficulty.Easy, 1, 11)]
    [InlineData(QuizDifficulty.Easy, 5, 15)]
    [InlineData(QuizDifficulty.Medium, 0, 15)]
    [InlineData(QuizDifficulty.Medium, 5, 23)] // 15 x 1.5 = 22.5, rounds away from zero to 23.
    [InlineData(QuizDifficulty.Hard, 0, 25)]
    [InlineData(QuizDifficulty.Hard, 5, 38)] // 25 x 1.5 = 37.5, rounds away from zero to 38.
    public void GetXpForCorrectAnswer_ScalesBaseXpByTheComboMultiplier(QuizDifficulty difficulty, int comboCount, int expectedXp)
    {
        QuizScoring.GetXpForCorrectAnswer(difficulty, comboCount).Should().Be(expectedXp);
    }

    [Fact]
    public void GetCoinReward_IsFlatPerDifficultyRegardlessOfCombo()
    {
        QuizScoring.GetCoinReward(QuizDifficulty.Easy).Should().Be(2);
        QuizScoring.GetCoinReward(QuizDifficulty.Medium).Should().Be(3);
        QuizScoring.GetCoinReward(QuizDifficulty.Hard).Should().Be(5);
    }
}
