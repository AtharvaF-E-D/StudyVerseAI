namespace StudyVerse.Domain.Entities;

/// <summary>
/// Records that a user has earned one of the fixed, in-code badge definitions (see
/// <see cref="StudyVerse.Domain.Gamification.BadgeCatalog"/>) — recognition only, no XP/coin
/// reward attached to earning a badge (unlike challenges/missions/daily rewards/spins, which do
/// award currency). A unique index on (UserId, BadgeId) prevents the same badge being recorded
/// twice for a user; <c>BadgeEvaluationService</c> is the only place rows are inserted here, and it
/// only ever inserts badges not already present for that user.
/// </summary>
public class UserBadge
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// Matches the <c>Id</c> of one of the static <see cref="StudyVerse.Domain.Gamification.BadgeDefinition"/>
    /// entries — not a foreign key to a database-backed table, since there is no badge template table.
    /// </summary>
    public Guid BadgeId { get; set; }

    public DateTime EarnedAtUtc { get; set; }

    public User? User { get; set; }
}
