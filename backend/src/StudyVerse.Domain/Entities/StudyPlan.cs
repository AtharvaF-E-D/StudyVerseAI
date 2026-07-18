using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// One user's AI-generated study schedule counting down to <see cref="ExamDate"/>. Only one plan
/// per user may be <see cref="StudyPlanStatus.Active"/> at a time —
/// <c>CreateStudyPlanCommandHandler</c> archives any existing active plan for the user before
/// creating a new one, so a user always has at most one live schedule to follow; older plans remain
/// in the table as <see cref="StudyPlanStatus.Archived"/> history rather than being deleted.
///
/// <see cref="SubjectsJson"/> and <see cref="WeakTopicsJson"/> store their string lists as JSON
/// array text columns rather than child tables — the same reasoning as <see cref="NoteContent"/>'s
/// <c>*Json</c> columns: these lists are small, always read/written as a single whole (nothing ever
/// needs to query "find all plans covering subject X"), and are wholly owned/replaced by this plan
/// rather than edited element-by-element, so normalizing them into their own tables would add join
/// complexity for no real benefit. See <c>StudyPlanJsonHelper</c> (Application layer) for the
/// serialize/deserialize round trip.
/// </summary>
public class StudyPlan
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateOnly ExamDate { get; set; }

    /// <summary>JSON array of strings — see this class's doc comment.</summary>
    public string SubjectsJson { get; set; } = "[]";

    /// <summary>JSON array of strings — see this class's doc comment.</summary>
    public string WeakTopicsJson { get; set; } = "[]";

    public int HoursPerDayMinutes { get; set; }

    public StudyPlanStatus Status { get; set; } = StudyPlanStatus.Active;

    public DateTime CreatedAtUtc { get; set; }

    public User? User { get; set; }

    public List<StudyPlanTask> Tasks { get; set; } = [];
}
