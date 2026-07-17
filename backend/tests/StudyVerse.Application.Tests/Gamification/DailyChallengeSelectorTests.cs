using FluentAssertions;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Gamification;

public sealed class DailyChallengeSelectorTests
{
    [Fact]
    public void GetTodaysTemplates_ForTheSameDate_ReturnsTheSameThreeTemplatesInTheSameOrder()
    {
        var date = new DateOnly(2026, 7, 17);

        var first = DailyChallengeSelector.GetTodaysTemplates(date);
        var second = DailyChallengeSelector.GetTodaysTemplates(date);

        first.Should().BeEquivalentTo(second, options => options.WithStrictOrdering());
    }

    [Fact]
    public void GetTodaysTemplates_AlwaysReturnsExactlyThreeDistinctTemplatesFromTheFixedCatalog()
    {
        var startDate = new DateOnly(2026, 1, 1);

        foreach (var offset in Enumerable.Range(0, 400))
        {
            var date = startDate.AddDays(offset);
            var templates = DailyChallengeSelector.GetTodaysTemplates(date);

            templates.Should().HaveCount(3);
            templates.Select(t => t.Id).Should().OnlyHaveUniqueItems();
            templates.Should().OnlyContain(t => ChallengeCatalog.All.Contains(t));
        }
    }

    [Fact]
    public void GetTodaysTemplates_RotatesToADifferentSetOnTheNextCalendarDay()
    {
        var today = new DateOnly(2026, 7, 17);
        var tomorrow = today.AddDays(1);

        var todaysTemplates = DailyChallengeSelector.GetTodaysTemplates(today);
        var tomorrowsTemplates = DailyChallengeSelector.GetTodaysTemplates(tomorrow);

        todaysTemplates.Should().NotBeEquivalentTo(tomorrowsTemplates, options => options.WithStrictOrdering());
    }
}
