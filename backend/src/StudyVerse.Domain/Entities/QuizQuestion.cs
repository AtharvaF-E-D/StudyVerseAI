using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// A single static trivia question in the Rapid Fire Quiz question bank. Seeded via EF Core
/// <c>HasData</c> (see <c>QuizQuestionSeedData</c> in Infrastructure) with stable hardcoded ids so
/// the seed stays idempotent across migrations — the same reasoning as the Phase 3
/// <c>ChallengeTemplate</c> static ids. Unlike <c>ChallengeTemplate</c>, this content lives in a
/// real table (not a fully in-code catalog): quiz questions need to be queried, filtered by
/// category/difficulty, randomized, and joined against <see cref="QuizSessionQuestion"/> for
/// anti-repetition, none of which an in-memory-only catalog supports well.
/// </summary>
public class QuizQuestion
{
    public Guid Id { get; set; }

    public string Category { get; set; } = string.Empty;

    public QuizDifficulty Difficulty { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;

    /// <summary>0-based index (0=A, 1=B, 2=C, 3=D) of the correct option. Never sent to the client
    /// before an answer is submitted for it — see <c>StartQuizSessionCommandHandler</c> and
    /// <c>GetQuizSessionQueryHandler</c>, which both project questions without this field.</summary>
    public int CorrectOptionIndex { get; set; }

    /// <summary>A one-sentence explanation of the correct answer, shown in the post-session review.</summary>
    public string Explanation { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
