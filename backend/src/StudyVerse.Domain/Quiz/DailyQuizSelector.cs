using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Quiz;

/// <summary>
/// Deterministically picks the same category+difficulty pair for every user on a given UTC
/// calendar day, rotating to the next pair the next day — the same
/// <c>(dayOfYear + year) % N</c> rotation style as <see cref="StudyVerse.Domain.Gamification.DailyChallengeSelector"/>.
/// Pure function of the date, so no persisted state is needed to know "what's today's daily quiz
/// challenge".
/// </summary>
public static class DailyQuizSelector
{
    // Every (category, difficulty) combination, in a fixed order, forms the rotation cycle.
    private static readonly IReadOnlyList<(string Category, QuizDifficulty Difficulty)> Pairs =
        QuizCategories.All
            .SelectMany(category => Enum.GetValues<QuizDifficulty>().Select(difficulty => (category, difficulty)))
            .ToList();

    public static (string Category, QuizDifficulty Difficulty) GetTodaysChallenge(DateOnly today)
    {
        var seed = (today.DayOfYear + today.Year) % Pairs.Count;
        return Pairs[seed];
    }
}
