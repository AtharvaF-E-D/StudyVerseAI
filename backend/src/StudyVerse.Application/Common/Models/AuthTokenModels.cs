namespace StudyVerse.Application.Common.Models;

/// <summary>A freshly minted JWT access token.</summary>
public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

/// <summary>
/// A freshly minted refresh token. <see cref="RawToken"/> is returned to the client and never
/// persisted; only <see cref="TokenHash"/> is stored.
/// </summary>
public sealed record RefreshTokenResult(string RawToken, string TokenHash, DateTime ExpiresAtUtc);

/// <summary>The token pair + user summary returned by every "issue session" flow (login, OTP, social, refresh).</summary>
public sealed record AuthSessionDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    UserSummaryDto User);

/// <summary>Token pair only, no user summary — returned by the token-refresh flow.</summary>
public sealed record TokenPairDto(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc);

public sealed record UserSummaryDto(Guid Id, string Email, string DisplayName, bool EmailVerified);

public sealed record UserProfileDto(
    Guid Id,
    string Email,
    string DisplayName,
    bool EmailVerified,
    DateTime CreatedAtUtc);

/// <summary>Result of verifying an external (Google/Apple) identity token.</summary>
public sealed record ExternalUserInfo(string Email, bool EmailVerified, string? DisplayName, string ProviderUserId);
