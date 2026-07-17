namespace StudyVerse.Domain.Enums;

/// <summary>Lifecycle state of a <see cref="StudyVerse.Domain.Entities.QuizSession"/>.</summary>
public enum QuizSessionStatus
{
    InProgress = 0,

    /// <summary>All questions were answered, or lives reached 0 — either way, the session ran to its end.</summary>
    Completed = 1,

    /// <summary>The user quit mid-session (<c>AbandonQuizSessionCommand</c>). No rewards are awarded.</summary>
    Abandoned = 2,
}
