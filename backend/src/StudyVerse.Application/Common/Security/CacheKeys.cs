using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Common.Security;

/// <summary>Centralized <see cref="Interfaces.ICacheService"/> key conventions.</summary>
public static class CacheKeys
{
    /// <summary>Holds the UTC instant a locked-out account may retry, as round-trip "O" format.</summary>
    public static string LoginLockoutUntil(Guid userId) => $"auth:lockout:{userId}";

    /// <summary>Presence-only marker enforcing the 1-request-per-60-seconds OTP throttle.</summary>
    public static string OtpRequestThrottle(OtpChannel channel, string destination) =>
        $"auth:otp:throttle:{channel}:{destination.Trim().ToLowerInvariant()}";
}
