namespace StudyVerse.Domain.Gamification;

/// <summary>
/// Deterministically picks 3 of the 6 fixed <see cref="ChallengeCatalog"/> templates to show as
/// "today's challenges" - the same 3 for every user on a given UTC calendar day, rotating to a
/// different set of 3 the next day. Pure function of the date, so no persisted state is needed.
/// </summary>
public static class DailyChallengeSelector
{
    private const int TemplatesPerDay = 3;

    public static IReadOnlyList<ChallengeTemplate> GetTodaysTemplates(DateOnly today)
    {
        var templates = ChallengeCatalog.All;
        var count = templates.Count;

        // A stable rotation seed derived from the calendar date: (dayOfYear + year) shifts the
        // starting offset by one template each day, and differs across years for the same
        // day-of-year, without needing any persisted or externally-supplied state.
        var seed = (today.DayOfYear + today.Year) % count;

        return Enumerable.Range(0, TemplatesPerDay)
            .Select(i => templates[(seed + i) % count])
            .ToList();
    }
}
