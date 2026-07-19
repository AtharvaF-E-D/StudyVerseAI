namespace StudyVerse.Domain.Gamification;

/// <summary>
/// Which real underlying table(s) a <see cref="MissionTemplate"/>'s progress is counted from — see
/// the Application layer's <c>MissionProgressService</c> for the exact query behind each value.
/// </summary>
public enum MissionMetric
{
    /// <summary>Count of <c>QuizSession</c> rows with Status=Completed, ended within the mission week.</summary>
    QuizSessionsCompleted,

    /// <summary>Count of distinct problem ids with an Accepted <c>CodeSubmission</c> within the mission week.</summary>
    CodingProblemsSolved,

    /// <summary>
    /// Distinct calendar days within the mission week with any real activity, checked across
    /// <c>QuizSession</c> (started), <c>CodeSubmission</c> (submitted), and <c>Flashcard</c>
    /// (last reviewed) — three representative activity tables, not every table, documented on the
    /// calculator itself.
    /// </summary>
    StudyDaysActive,

    /// <summary>
    /// Count of <c>Flashcard</c> rows last-reviewed within the mission week. <c>ReviewCardCommandHandler</c>
    /// only persists the most recent review timestamp per card (no per-review event log table
    /// exists), so a card reviewed more than once in the same week still only counts once — a
    /// documented approximation of "reviews", not an exact review-event count.
    /// </summary>
    FlashcardsReviewed,

    /// <summary>Count of <c>NewsBookmark</c> rows created within the mission week.</summary>
    NewsArticlesBookmarked,
}

/// <summary>A fixed, in-code weekly mission definition — mirrors <see cref="ChallengeTemplate"/>'s static-catalog pattern.</summary>
public sealed record MissionTemplate(
    Guid Id,
    string Title,
    string Description,
    MissionMetric Metric,
    int TargetCount,
    int XpReward,
    int CoinReward);

/// <summary>
/// The complete, fixed set of weekly mission templates. Ids are stable hardcoded GUIDs, same
/// reasoning as <see cref="BadgeCatalog"/>/<see cref="ChallengeCatalog"/>.
/// <see cref="WeeklyMissionSelector"/> picks 3 of these 5 as "this week's active missions".
/// </summary>
public static class MissionCatalog
{
    public static readonly Guid QuizGrinderId = Guid.Parse("55555555-5555-5555-5555-555555550001");
    public static readonly Guid CodeSprintId = Guid.Parse("55555555-5555-5555-5555-555555550002");
    public static readonly Guid ConsistencyCountsId = Guid.Parse("55555555-5555-5555-5555-555555550003");
    public static readonly Guid FlashcardFocusId = Guid.Parse("55555555-5555-5555-5555-555555550004");
    public static readonly Guid StayInformedId = Guid.Parse("55555555-5555-5555-5555-555555550005");

    public static readonly IReadOnlyList<MissionTemplate> All =
    [
        new(QuizGrinderId, "Quiz Grinder", "Complete 3 quiz sessions this week.", MissionMetric.QuizSessionsCompleted, TargetCount: 3, XpReward: 40, CoinReward: 20),
        new(CodeSprintId, "Code Sprint", "Solve 2 coding problems this week.", MissionMetric.CodingProblemsSolved, TargetCount: 2, XpReward: 50, CoinReward: 25),
        new(ConsistencyCountsId, "Consistency Counts", "Study on 3 different days this week.", MissionMetric.StudyDaysActive, TargetCount: 3, XpReward: 30, CoinReward: 15),
        new(FlashcardFocusId, "Flashcard Focus", "Review 20 flashcards this week.", MissionMetric.FlashcardsReviewed, TargetCount: 20, XpReward: 35, CoinReward: 15),
        new(StayInformedId, "Stay Informed", "Bookmark 2 news articles this week.", MissionMetric.NewsArticlesBookmarked, TargetCount: 2, XpReward: 20, CoinReward: 10),
    ];
}
