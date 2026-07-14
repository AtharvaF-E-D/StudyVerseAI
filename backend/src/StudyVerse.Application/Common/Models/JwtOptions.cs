namespace StudyVerse.Application.Common.Models;

/// <summary>
/// Bound from the "Jwt" configuration section. Shared shape between the token-issuing service
/// (Infrastructure) and the token-validating middleware (Api) so both sides always agree on the
/// signing key/issuer/audience.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 30;
}
