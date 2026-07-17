namespace StudyVerse.Domain.Gamification;

/// <summary>
/// A fixed, in-code daily challenge definition. There is no admin CMS for challenges yet (planned
/// for a later phase), so these live as a hardcoded catalog rather than a database table.
/// </summary>
public sealed record ChallengeTemplate(Guid Id, string Title, string Description, int XpReward, int CoinReward);

/// <summary>
/// The complete, fixed set of daily challenge templates. Ids are stable hardcoded GUIDs so they
/// stay consistent across app restarts and database migrations — <see cref="Entities.ChallengeCompletion.ChallengeTemplateId"/>
/// references them by value. Templates are intentionally generic and feature-agnostic: quizzes,
/// flashcards, notes, and the study planner don't exist yet, so nothing here references them.
/// </summary>
public static class ChallengeCatalog
{
    public static readonly IReadOnlyList<ChallengeTemplate> All =
    [
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111101"),
            "Open StudyVerse Today",
            "Show up and open the app - the first step to building a habit.",
            XpReward: 10,
            CoinReward: 5),
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111102"),
            "Review Your Goals for the Week",
            "Take a moment to look over what you want to accomplish this week.",
            XpReward: 15,
            CoinReward: 5),
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111103"),
            "Take a 5-Minute Study Break",
            "Step away, stretch, and reset your focus for five minutes.",
            XpReward: 10,
            CoinReward: 5),
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111104"),
            "Plan Tomorrow's Session",
            "Jot down what you'd like to focus on tomorrow so future-you has a head start.",
            XpReward: 15,
            CoinReward: 5),
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111105"),
            "Read One New Thing",
            "Learn something new today, however small.",
            XpReward: 15,
            CoinReward: 5),
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111106"),
            "Reflect on Today's Progress",
            "Take a minute to note what went well today.",
            XpReward: 10,
            CoinReward: 5),
    ];
}
