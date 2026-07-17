using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// One playthrough of the Rapid Fire Quiz: 10 questions from a single category+difficulty, 3
/// lives, and a consecutive-correct combo that multiplies XP (see
/// <see cref="StudyVerse.Domain.Quiz.QuizScoring"/>). Ends (<see cref="Status"/> =
/// <see cref="QuizSessionStatus.Completed"/>) when all questions are answered or lives reach 0,
/// whichever happens first — <c>SubmitAnswerCommandHandler</c> is the only place XP/coins are
/// credited to <see cref="UserProgress"/>, and only once, at that completion moment, so
/// resuming/retrying never double-counts rewards.
/// </summary>
public class QuizSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Category { get; set; } = string.Empty;

    public QuizDifficulty Difficulty { get; set; }

    public QuizSessionStatus Status { get; set; } = QuizSessionStatus.InProgress;

    public int Lives { get; set; }

    public int CurrentQuestionIndex { get; set; }

    /// <summary>Consecutive-correct-answer counter. Resets to 0 on a wrong answer.</summary>
    public int ComboCount { get; set; }

    public int BestComboThisSession { get; set; }

    /// <summary>
    /// Raw quiz points accumulated from correct answers only (base XP by difficulty × the combo
    /// multiplier), never including the daily-challenge completion bonus. This is the "your score
    /// this game" number shown during play; <see cref="XpEarned"/> is the (possibly larger) amount
    /// actually credited to <see cref="UserProgress"/>.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// What gets credited to <see cref="UserProgress.Xp"/> once, at session completion: equal to
    /// <see cref="Score"/> plus the flat daily-challenge bonus if <see cref="IsDailyChallenge"/>
    /// and the session completed successfully.
    /// </summary>
    public int XpEarned { get; set; }

    /// <summary>Coins credited to <see cref="UserProgress.Coins"/> at completion: the flat
    /// per-correct-answer coin reward (not combo-scaled), plus the daily-challenge coin bonus if applicable.</summary>
    public int CoinsEarned { get; set; }

    /// <summary>True once the fifty-fifty power-up has been used this session (at most once per session).</summary>
    public bool UsedFiftyFifty { get; set; }

    /// <summary>True once the extra-time power-up has been used this session (at most once per session).</summary>
    public bool UsedExtraTime { get; set; }

    public bool IsDailyChallenge { get; set; }

    /// <summary>
    /// Set only when <see cref="IsDailyChallenge"/> is true, to the UTC calendar date the session
    /// was started. Null for ordinary sessions. A unique index on (UserId, DailyChallengeDateUtc)
    /// enforces "at most one daily-challenge session per user per UTC day" — Postgres treats NULLs
    /// as distinct in a unique index, so ordinary (non-daily) sessions never collide with each
    /// other, and no partial/filtered index is needed.
    /// </summary>
    public DateOnly? DailyChallengeDateUtc { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? EndedAtUtc { get; set; }

    public User? User { get; set; }

    public List<QuizSessionQuestion> Questions { get; set; } = [];
}
