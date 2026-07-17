namespace StudyVerse.Domain.Entities;

/// <summary>
/// Records that a user completed one of the fixed, in-code daily challenge templates (see
/// <see cref="StudyVerse.Domain.Gamification.ChallengeCatalog"/>) on a given UTC calendar date.
/// A unique index on (UserId, ChallengeTemplateId, CompletedDateUtc) prevents completing the same
/// challenge twice in one day.
/// </summary>
public class ChallengeCompletion
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// Matches the <c>Id</c> of one of the static <see cref="StudyVerse.Domain.Gamification.ChallengeTemplate"/>
    /// entries — not a foreign key to a database-backed table, since there is no template table.
    /// </summary>
    public Guid ChallengeTemplateId { get; set; }

    public DateOnly CompletedDateUtc { get; set; }

    public DateTime CompletedAtUtc { get; set; }

    public int XpAwarded { get; set; }

    public int CoinsAwarded { get; set; }

    public User? User { get; set; }
}
