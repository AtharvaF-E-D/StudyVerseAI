using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// One practice interview: 5 randomly-selected <see cref="InterviewQuestion"/>s of a single
/// <see cref="Type"/>, answered one at a time (each graded immediately by
/// <c>SubmitAnswerCommandHandler</c> — see <see cref="InterviewAnswer"/>) and finished off by
/// <c>CompleteInterviewSessionCommandHandler</c>, which averages the five per-answer scores into
/// <see cref="OverallScore"/> and synthesizes <see cref="ImprovementPlan"/> from all five Q&amp;A
/// pairs in one more AI call.
/// </summary>
public class InterviewSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public InterviewQuestionType Type { get; set; }

    public InterviewSessionStatus Status { get; set; } = InterviewSessionStatus.InProgress;

    /// <summary>
    /// JSON array of the exact <see cref="InterviewQuestion"/> ids selected for this session, in
    /// the fixed order they're presented — the small (~12-per-type) seeded pool makes a dedicated
    /// join table unnecessary; this is the same "own the small ordered set as a JSON text column"
    /// choice <c>CodingProblem.StarterCodeJson</c>/<c>NoteContent</c>'s <c>*Json</c> columns make.
    /// <c>SubmitAnswerCommandHandler</c> checks a submitted <c>QuestionId</c> against this list to
    /// reject a question that isn't part of the session; <c>GetInterviewSessionQueryHandler</c>
    /// deserializes it to join back against <see cref="InterviewQuestion"/> and any
    /// <see cref="InterviewAnswer"/>s given so far.
    /// </summary>
    public string SelectedQuestionIdsJson { get; set; } = "[]";

    /// <summary>0-100, set once by <c>CompleteInterviewSessionCommandHandler</c>: the average of the
    /// five 0-10 <see cref="InterviewAnswer.AiScore"/> values, scaled up by 10.</summary>
    public int? OverallScore { get; set; }

    /// <summary>2-3 concrete paragraphs synthesized from all five Q&amp;A pairs (plus their scores
    /// and feedback) in one <c>IAiChatProvider</c> call at completion — never generic filler, see
    /// <c>InterviewImprovementPlanPromptBuilder</c>.</summary>
    public string? ImprovementPlan { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public User? User { get; set; }

    public ICollection<InterviewAnswer> Answers { get; set; } = new List<InterviewAnswer>();
}
