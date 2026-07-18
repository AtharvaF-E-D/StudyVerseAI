namespace StudyVerse.Domain.Enums;

/// <summary>
/// <see cref="Rescheduled"/> is reserved for a future manual "user drags a task to a new date"
/// feature that this pass does not implement. The automatic missed-task recovery pass
/// (<c>MissedTaskRecoveryService</c>) never sets it: it marks the original overdue row
/// <see cref="Missed"/> (a terminal status — that row is never touched again) and creates a brand
/// new <see cref="Entities.StudyPlanTask"/> row (<see cref="Pending"/>, with
/// <see cref="Entities.StudyPlanTask.OriginalScheduledDateUtc"/> set) for the make-up session,
/// rather than mutating the missed row in place.
/// </summary>
public enum StudyPlanTaskStatus
{
    Pending = 0,
    Completed = 1,
    Missed = 2,
    Rescheduled = 3,
}
