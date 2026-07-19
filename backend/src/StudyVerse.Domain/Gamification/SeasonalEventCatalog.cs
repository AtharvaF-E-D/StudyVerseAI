namespace StudyVerse.Domain.Gamification;

/// <summary>
/// A fixed, in-code seasonal event window with a single bonus action - deliberately the simplest
/// possible version of "seasonal events" (a named banner + one flat bonus on the daily reward
/// claim), not a general XP-multiplier system retrofitted across every already-shipped feature.
/// That broader version was explicitly called out as disproportionate for this time-boxed pass;
/// this gives a real, working seasonal hook without touching Quiz/Flashcards/Coding/etc.
/// </summary>
public sealed record SeasonalEvent(string Name, string Description, DateOnly StartDateUtc, DateOnly EndDateUtc, int DailyRewardBonusCoins);

public static class SeasonalEventCatalog
{
    public static readonly IReadOnlyList<SeasonalEvent> All =
    [
        // Spans the initial rollout window so the event is actually observable in a real curl
        // walkthrough right after this phase ships.
        new(
            "Exam Season Sprint",
            "A limited-time coin bonus on top of your daily reward claim.",
            StartDateUtc: new DateOnly(2026, 7, 1),
            EndDateUtc: new DateOnly(2026, 8, 31),
            DailyRewardBonusCoins: 15),
    ];

    /// <summary>The currently-running event for <paramref name="today"/>, if any. Events don't overlap in this catalog, so at most one is ever active.</summary>
    public static SeasonalEvent? GetActiveEvent(DateOnly today) =>
        All.FirstOrDefault(e => today >= e.StartDateUtc && today <= e.EndDateUtc);
}
