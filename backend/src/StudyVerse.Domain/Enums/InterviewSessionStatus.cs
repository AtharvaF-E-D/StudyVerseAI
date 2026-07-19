namespace StudyVerse.Domain.Enums;

/// <summary>Lifecycle state of a <see cref="StudyVerse.Domain.Entities.InterviewSession"/>.</summary>
public enum InterviewSessionStatus
{
    InProgress = 0,

    /// <summary>All questions answered and graded, <c>OverallScore</c>/<c>ImprovementPlan</c>
    /// populated. Terminal — a completed session can never accept another answer.</summary>
    Completed = 1,
}
