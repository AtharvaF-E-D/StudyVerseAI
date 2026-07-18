using StudyVerse.Domain.Quiz;

namespace StudyVerse.Domain.MockTests;

/// <summary>
/// A fixed, in-code exam definition layered over the existing Rapid Fire Quiz question bank (see
/// <c>QuizQuestionSeedData</c> in Infrastructure) — no new question content is needed for Mock
/// Tests, just a "pull N questions from category X, give the student Y minutes" wrapper. There is
/// no admin CMS for mock test templates yet, so — exactly like
/// <see cref="StudyVerse.Domain.Gamification.ChallengeTemplate"/> — these live as a hardcoded
/// catalog rather than a database table.
/// </summary>
public sealed record MockTestTemplate(
    Guid Id,
    string Title,
    string Description,
    string Category,
    int QuestionCount,
    int DurationMinutes);

/// <summary>
/// The complete, fixed set of mock test templates. Ids are stable hardcoded GUIDs so they stay
/// consistent across app restarts and database migrations — <see cref="Entities.MockTestAttempt.TemplateId"/>
/// references them by value, not a foreign key to a database-backed table, since there is no
/// template table (same reasoning as <see cref="Entities.ChallengeCompletion.ChallengeTemplateId"/>).
/// </summary>
public static class MockTestCatalog
{
    /// <summary>
    /// Pseudo-category meaning "pull questions from all 5 real <see cref="QuizCategories"/>" — not
    /// itself one of the seeded quiz-question categories, so callers must special-case it rather
    /// than passing it straight through to a <c>QuizQuestions.Where(q => q.Category == ...)</c> filter.
    /// </summary>
    public const string MixedCategory = "Mixed";

    public static readonly IReadOnlyList<MockTestTemplate> All =
    [
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333301"),
            "Science Mock Test",
            "15 questions spanning easy, medium, and hard Science trivia — a full-length timed exam simulation.",
            QuizCategories.Science,
            QuestionCount: 15,
            DurationMinutes: 20),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333302"),
            "Mathematics Mastery Test",
            "15 Mathematics questions across all difficulty levels to test your numerical reasoning under time pressure.",
            QuizCategories.Mathematics,
            QuestionCount: 15,
            DurationMinutes: 20),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333303"),
            "Geography Explorer Test",
            "15 questions covering world geography, from capital cities to physical landmarks.",
            QuizCategories.Geography,
            QuestionCount: 15,
            DurationMinutes: 20),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333304"),
            "General Knowledge Full Test",
            "18 General Knowledge questions - the complete seeded bank for this category, for a thorough all-round check.",
            QuizCategories.GeneralKnowledge,
            QuestionCount: 18,
            DurationMinutes: 25),
        new(
            Guid.Parse("33333333-3333-3333-3333-333333333305"),
            "Mixed Subjects Grand Test",
            "20 questions drawn from all 5 categories - Science, Mathematics, History, Geography, and General Knowledge.",
            MixedCategory,
            QuestionCount: 20,
            DurationMinutes: 30),
    ];
}
