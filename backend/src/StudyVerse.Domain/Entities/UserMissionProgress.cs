namespace StudyVerse.Domain.Entities;

/// <summary>
/// One user's progress toward one of this week's active mission templates (see
/// <see cref="StudyVerse.Domain.Gamification.MissionCatalog"/>/<see cref="StudyVerse.Domain.Gamification.WeeklyMissionSelector"/>),
/// for the ISO week starting <see cref="WeekStartDateUtc"/> (a Monday). <see cref="CurrentCount"/>
/// is recomputed from the real underlying tables every time <c>GetMissionsQuery</c> (or the
/// gamification summary) runs — this row is a cache of the last computation plus the
/// completed/reward bookkeeping, not the source of truth for progress itself. A unique index on
/// (UserId, MissionTemplateId, WeekStartDateUtc) means a new row is created each time a template
/// rotates back into the active set for a new week, rather than reusing a stale one.
/// </summary>
public class UserMissionProgress
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// Matches the <c>Id</c> of one of the static <see cref="StudyVerse.Domain.Gamification.MissionTemplate"/>
    /// entries — not a foreign key to a database-backed table, since there is no mission template table.
    /// </summary>
    public Guid MissionTemplateId { get; set; }

    /// <summary>The Monday (UTC calendar date) of the ISO week this progress row is for.</summary>
    public DateOnly WeekStartDateUtc { get; set; }

    public int CurrentCount { get; set; }

    public bool IsCompleted { get; set; }

    /// <summary>
    /// Set exactly once, the moment <see cref="IsCompleted"/> first flips from false to true — the
    /// same moment XP/coins are credited to <see cref="UserProgress"/>. Never cleared afterward,
    /// even if <see cref="CurrentCount"/> is later recomputed to something below the target (e.g. if
    /// underlying activity were somehow removed) — a mission, once completed, stays completed for the week.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    public User? User { get; set; }
}
