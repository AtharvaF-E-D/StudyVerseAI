using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    /// <summary>Normalized (lower-cased, trimmed) email address. Unique across all users.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Null for accounts that only ever signed up via a social provider.</summary>
    public string? PasswordHash { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool EmailVerified { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    /// <summary>The provider used at signup time. Does not preclude later linking other providers.</summary>
    public AuthProvider AuthProvider { get; set; }

    public bool IsLockedOut { get; set; }

    public int FailedLoginAttempts { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = [];

    public List<UserToken> UserTokens { get; set; } = [];
}
