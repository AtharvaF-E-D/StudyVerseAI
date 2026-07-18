namespace StudyVerse.Domain.Enums;

/// <summary>
/// A user may have many <see cref="Entities.StudyPlan"/> rows over time, but only one
/// <see cref="Active"/> at once — <c>CreateStudyPlanCommandHandler</c> archives whatever was
/// previously Active before creating a new plan, so old plans stick around as history rather than
/// being deleted.
/// </summary>
public enum StudyPlanStatus
{
    Active = 0,
    Archived = 1,
}
