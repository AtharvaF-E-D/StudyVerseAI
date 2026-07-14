namespace StudyVerse.Domain.Entities;

/// <summary>
/// A rotating refresh token bound to a single device. Raw token values are never persisted —
/// only their SHA-256 hash — so a database leak does not expose usable tokens.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public string DeviceId { get; set; } = string.Empty;

    public string? DeviceName { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    /// <summary>
    /// The hash of the token that replaced this one during rotation. Used to walk the rotation
    /// chain and, combined with <see cref="RevokedAtUtc"/>, to detect reuse of a stale token.
    /// </summary>
    public string? ReplacedByTokenHash { get; set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsExpired(DateTime utcNow) => ExpiresAtUtc <= utcNow;

    public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);

    public User? User { get; set; }
}
