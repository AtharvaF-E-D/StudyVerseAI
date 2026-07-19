namespace StudyVerse.Domain.Entities;

/// <summary>
/// One daily spin of the prize wheel (see <see cref="StudyVerse.Domain.Gamification.SpinPrizeCatalog"/>
/// for the fixed weighted prize table). <see cref="SpinDateUtc"/> is the UTC calendar date, used
/// for the "already spun today" check (a unique index on (UserId, SpinDateUtc) enforces it);
/// <see cref="SpunAtUtc"/> is the full timestamp, kept separately for display/history ordering.
/// </summary>
public class SpinResult
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateOnly SpinDateUtc { get; set; }

    public DateTime SpunAtUtc { get; set; }

    public string PrizeLabel { get; set; } = string.Empty;

    public int CoinsAwarded { get; set; }

    public int XpAwarded { get; set; }

    public User? User { get; set; }
}
