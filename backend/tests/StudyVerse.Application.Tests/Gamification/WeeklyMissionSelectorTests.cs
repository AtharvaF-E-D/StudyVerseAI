using FluentAssertions;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Gamification;

public sealed class WeeklyMissionSelectorTests
{
    // January 1, 2001 is a verified Monday (confirmed via Zeller's congruence), used as a safe,
    // unambiguous ISO-week anchor rather than guessing a day-of-week for some other date.
    private static readonly DateOnly AVerifiedMonday = new(2001, 1, 1);

    [Fact]
    public void GetThisWeeksTemplates_ForTheSameWeek_ReturnsTheSameThreeTemplatesInTheSameOrder()
    {
        var sundayOfSameWeek = AVerifiedMonday.AddDays(6);

        var first = WeeklyMissionSelector.GetThisWeeksTemplates(AVerifiedMonday);
        var second = WeeklyMissionSelector.GetThisWeeksTemplates(sundayOfSameWeek);

        first.Should().BeEquivalentTo(second, options => options.WithStrictOrdering());
    }

    [Fact]
    public void GetThisWeeksTemplates_AlwaysReturnsExactlyThreeDistinctTemplatesFromTheFixedCatalog()
    {
        var startDate = new DateOnly(2026, 1, 1);

        foreach (var offset in Enumerable.Range(0, 400))
        {
            var date = startDate.AddDays(offset);
            var templates = WeeklyMissionSelector.GetThisWeeksTemplates(date);

            templates.Should().HaveCount(3);
            templates.Select(t => t.Id).Should().OnlyHaveUniqueItems();
            templates.Should().OnlyContain(t => MissionCatalog.All.Contains(t));
        }
    }

    [Fact]
    public void GetThisWeeksTemplates_RotatesToADifferentSetOnTheFollowingWeek()
    {
        var nextWeek = AVerifiedMonday.AddDays(7);

        var thisWeeksTemplates = WeeklyMissionSelector.GetThisWeeksTemplates(AVerifiedMonday);
        var nextWeeksTemplates = WeeklyMissionSelector.GetThisWeeksTemplates(nextWeek);

        thisWeeksTemplates.Should().NotBeEquivalentTo(nextWeeksTemplates, options => options.WithStrictOrdering());
    }

    [Fact]
    public void GetWeekStartDateUtc_ForTheMondayItself_ReturnsTheSameDate()
    {
        WeeklyMissionSelector.GetWeekStartDateUtc(AVerifiedMonday).Should().Be(AVerifiedMonday);
    }

    [Fact]
    public void GetWeekStartDateUtc_ForATuesdayMidweek_ReturnsThePrecedingMonday()
    {
        WeeklyMissionSelector.GetWeekStartDateUtc(AVerifiedMonday.AddDays(1)).Should().Be(AVerifiedMonday);
    }

    [Fact]
    public void GetWeekStartDateUtc_ForTheSundayEndOfWeek_ReturnsTheSameWeeksMonday()
    {
        WeeklyMissionSelector.GetWeekStartDateUtc(AVerifiedMonday.AddDays(6)).Should().Be(AVerifiedMonday);
    }

    [Fact]
    public void GetWeekStartDateUtc_ForTheFollowingMonday_AdvancesToTheNextWeek()
    {
        var nextMonday = AVerifiedMonday.AddDays(7);

        WeeklyMissionSelector.GetWeekStartDateUtc(nextMonday).Should().Be(nextMonday);
    }
}
