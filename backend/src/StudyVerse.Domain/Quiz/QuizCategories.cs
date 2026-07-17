namespace StudyVerse.Domain.Quiz;

/// <summary>
/// The fixed set of categories the Rapid Fire Quiz question bank is seeded under (see
/// <c>QuizQuestionSeedData</c> in Infrastructure). Commands validate a requested category against
/// this list rather than trusting arbitrary client input — the same "in-code catalog is the
/// source of truth for valid ids/values" idea as <c>Gamification.ChallengeCatalog</c>.
/// </summary>
public static class QuizCategories
{
    public const string Science = "Science";
    public const string Mathematics = "Mathematics";
    public const string History = "History";
    public const string Geography = "Geography";
    public const string GeneralKnowledge = "General Knowledge";

    public static readonly IReadOnlyList<string> All =
    [
        Science,
        Mathematics,
        History,
        Geography,
        GeneralKnowledge,
    ];
}
