using System.Globalization;

namespace StudyVerse.Domain.Gamification;

/// <summary>
/// Deterministically picks 3 of the 5 fixed <see cref="MissionCatalog"/> templates to show as
/// "this week's missions" - the same 3 for every user in a given ISO week, rotating to a different
/// set of 3 the next week. Pure function of the date, so no persisted state is needed - mirrors
/// <see cref="DailyChallengeSelector"/> exactly, but rotates weekly (keyed off the ISO week number)
/// instead of daily (keyed off day-of-year).
/// </summary>
public static class WeeklyMissionSelector
{
    private const int TemplatesPerWeek = 3;

    public static IReadOnlyList<MissionTemplate> GetThisWeeksTemplates(DateOnly today)
    {
        var templates = MissionCatalog.All;
        var count = templates.Count;

        // A stable rotation seed derived from the ISO week: (isoWeek + year) shifts the starting
        // offset by one template each week, and differs across years for the same ISO week number,
        // without needing any persisted or externally-supplied state - the weekly analogue of
        // DailyChallengeSelector's (dayOfYear + year) seed.
        var isoWeek = ISOWeek.GetWeekOfYear(today.ToDateTime(TimeOnly.MinValue));
        var seed = (isoWeek + today.Year) % count;

        return Enumerable.Range(0, TemplatesPerWeek)
            .Select(i => templates[(seed + i) % count])
            .ToList();
    }

    /// <summary>
    /// The Monday (UTC calendar date) of the ISO week containing <paramref name="today"/>. ISO
    /// weeks run Monday-Sunday; <see cref="DateOnly.DayOfWeek"/> numbers Sunday as 0, so it's
    /// remapped to the 1(Monday)-7(Sunday) ISO numbering before subtracting back to Monday.
    /// </summary>
    public static DateOnly GetWeekStartDateUtc(DateOnly today)
    {
        var isoDayOfWeek = today.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)today.DayOfWeek;
        return today.AddDays(-(isoDayOfWeek - 1));
    }
}
