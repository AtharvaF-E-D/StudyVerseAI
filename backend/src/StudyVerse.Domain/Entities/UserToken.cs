using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// A single-use, long-lived token issued to a user for out-of-band flows (email verification,
/// password reset). Folded into one entity, distinguished by <see cref="Purpose"/>, to avoid
/// duplicating near-identical tables for each flow.
/// </summary>
public class UserToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public UserTokenPurpose Purpose { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? ConsumedAtUtc { get; set; }

    public bool IsConsumed => ConsumedAtUtc.HasValue;

    public bool IsExpired(DateTime utcNow) => ExpiresAtUtc <= utcNow;

    public bool IsValid(DateTime utcNow) => !IsConsumed && !IsExpired(utcNow);

    public User? User { get; set; }
}
