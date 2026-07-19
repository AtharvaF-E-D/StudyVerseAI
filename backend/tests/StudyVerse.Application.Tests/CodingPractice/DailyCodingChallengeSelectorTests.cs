using FluentAssertions;
using StudyVerse.Domain.CodingPractice;

namespace StudyVerse.Application.Tests.CodingPractice;

public sealed class DailyCodingChallengeSelectorTests
{
    private static List<Guid> BuildPool(int count) =>
        Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();

    [Fact]
    public void GetTodaysProblemId_ForTheSameDate_ReturnsTheSameProblemEveryTime()
    {
        var pool = BuildPool(26);
        var date = new DateOnly(2026, 7, 17);

        var first = DailyCodingChallengeSelector.GetTodaysProblemId(pool, date);
        var second = DailyCodingChallengeSelector.GetTodaysProblemId(pool, date);

        first.Should().Be(second);
    }

    [Fact]
    public void GetTodaysProblemId_MatchesTheDocumentedDayOfYearPlusYearModuloFormula()
    {
        var pool = BuildPool(26);
        var date = new DateOnly(2026, 7, 17);

        var expectedIndex = (date.DayOfYear + date.Year) % pool.Count;

        DailyCodingChallengeSelector.GetTodaysProblemId(pool, date).Should().Be(pool[expectedIndex]);
    }

    [Fact]
    public void GetTodaysProblemId_AlwaysReturnsAProblemFromThePool()
    {
        var pool = BuildPool(26);
        var startDate = new DateOnly(2026, 1, 1);

        foreach (var offset in Enumerable.Range(0, 400))
        {
            var selected = DailyCodingChallengeSelector.GetTodaysProblemId(pool, startDate.AddDays(offset));
            pool.Should().Contain(selected);
        }
    }

    [Fact]
    public void GetTodaysProblemId_RotatesToADifferentProblemOnAnotherDay()
    {
        var pool = BuildPool(26);
        var today = new DateOnly(2026, 7, 17);
        var tomorrow = today.AddDays(1);

        var todays = DailyCodingChallengeSelector.GetTodaysProblemId(pool, today);
        var tomorrows = DailyCodingChallengeSelector.GetTodaysProblemId(pool, tomorrow);

        todays.Should().NotBe(tomorrows);
    }

    [Fact]
    public void GetTodaysProblemId_RotatesAcrossTheYear_VisitingMoreThanOneProblem()
    {
        var pool = BuildPool(26);
        var startDate = new DateOnly(2026, 1, 1);

        var distinctProblems = Enumerable.Range(0, 60)
            .Select(offset => DailyCodingChallengeSelector.GetTodaysProblemId(pool, startDate.AddDays(offset)))
            .Distinct()
            .ToList();

        distinctProblems.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void GetTodaysProblemId_EmptyPool_Throws()
    {
        var act = () => DailyCodingChallengeSelector.GetTodaysProblemId([], new DateOnly(2026, 7, 17));

        act.Should().Throw<ArgumentException>();
    }
}
