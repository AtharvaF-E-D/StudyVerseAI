namespace StudyVerse.Domain.Enums;

/// <summary>Lifecycle state of a <see cref="StudyVerse.Domain.Entities.MockTestAttempt"/>.</summary>
public enum MockTestAttemptStatus
{
    InProgress = 0,

    /// <summary>Graded: every question has been scored server-side and <c>Score</c>/<c>PercentileRank</c>/
    /// <c>AiWeaknessAnalysis</c> are populated. Terminal — a submitted attempt can never be resubmitted.</summary>
    Submitted = 1,
}
