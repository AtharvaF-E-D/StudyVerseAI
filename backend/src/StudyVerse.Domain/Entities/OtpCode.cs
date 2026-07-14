using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// A short-lived, numeric one-time-passcode sent to an email address or phone number.
/// Only the hash of the code is persisted.
/// </summary>
public class OtpCode
{
    public Guid Id { get; set; }

    /// <summary>The email address or phone number the code was sent to.</summary>
    public string Destination { get; set; } = string.Empty;

    public OtpChannel Channel { get; set; }

    public string CodeHash { get; set; } = string.Empty;

    public OtpPurpose Purpose { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? ConsumedAtUtc { get; set; }

    public int AttemptCount { get; set; }

    public bool IsConsumed => ConsumedAtUtc.HasValue;

    public bool IsExpired(DateTime utcNow) => ExpiresAtUtc <= utcNow;
}
