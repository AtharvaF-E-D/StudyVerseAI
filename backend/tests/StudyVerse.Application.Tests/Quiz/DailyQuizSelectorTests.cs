using FluentAssertions;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Tests.Quiz;

public sealed class DailyQuizSelectorTests
{
    [Fact]
    public void GetTodaysChallenge_ForTheSameDate_ReturnsTheSamePairEveryTime()
    {
        var date = new DateOnly(2026, 7, 17);

        var first = DailyQuizSelector.GetTodaysChallenge(date);
        var second = DailyQuizSelector.GetTodaysChallenge(date);

        first.Should().Be(second);
    }

    [Fact]
    public void GetTodaysChallenge_AlwaysReturnsAValidCategoryAndDifficulty()
    {
        var startDate = new DateOnly(2026, 1, 1);

        foreach (var offset in Enumerable.Range(0, 400))
        {
            var (category, difficulty) = DailyQuizSelector.GetTodaysChallenge(startDate.AddDays(offset));

            QuizCategories.All.Should().Contain(category);
            Enum.IsDefined(difficulty).Should().BeTrue();
        }
    }

    [Fact]
    public void GetTodaysChallenge_RotatesAcrossTheYear_VisitingMoreThanOnePair()
    {
        var startDate = new DateOnly(2026, 1, 1);

        var distinctPairs = Enumerable.Range(0, 30)
            .Select(offset => DailyQuizSelector.GetTodaysChallenge(startDate.AddDays(offset)))
            .Distinct()
            .ToList();

        distinctPairs.Count.Should().BeGreaterThan(1);
    }
}
