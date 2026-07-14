using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Infrastructure.Options;

namespace StudyVerse.Infrastructure.External;

/// <summary>
/// Validates a Sign in with Apple identity token by fetching Apple's JWKS
/// (https://appleid.apple.com/auth/keys) and checking the token's signature, issuer, audience
/// and expiry. The JWKS response is cached for 24h (via <see cref="ICacheService"/>) since it
/// changes rarely and Apple rate-limits that endpoint.
/// </summary>
public sealed class AppleTokenValidator : IAppleTokenValidator
{
    private const string AppleIssuer = "https://appleid.apple.com";
    private const string JwksUrl = "https://appleid.apple.com/auth/keys";
    private const string JwksCacheKey = "apple:jwks";
    private static readonly TimeSpan JwksCacheDuration = TimeSpan.FromHours(24);

    private readonly HttpClient _httpClient;
    private readonly ICacheService _cache;
    private readonly AppleOptions _options;
    private readonly ILogger<AppleTokenValidator> _logger;

    public AppleTokenValidator(
        IHttpClientFactory httpClientFactory,
        ICacheService cache,
        IOptions<AppleOptions> options,
        ILogger<AppleTokenValidator> logger)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(AppleTokenValidator));
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ExternalUserInfo?> ValidateAsync(string identityToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var jwks = await GetJsonWebKeySetAsync(cancellationToken);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AppleIssuer,
                ValidateAudience = true,
                ValidAudience = _options.ClientId,
                ValidateLifetime = true,
                IssuerSigningKeys = jwks.GetSigningKeys(),
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(identityToken, validationParameters, out _);

            var email = principal.FindFirst("email")?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Apple identity token did not contain an email claim.");
                return null;
            }

            var emailVerifiedClaim = principal.FindFirst("email_verified")?.Value;
            var emailVerified = string.Equals(emailVerifiedClaim, "true", StringComparison.OrdinalIgnoreCase);
            var subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;

            return new ExternalUserInfo(email, emailVerified, null, subject);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Apple identity token validation failed.");
            return null;
        }
    }

    private async Task<JsonWebKeySet> GetJsonWebKeySetAsync(CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync(JwksCacheKey, cancellationToken);
        if (cached is not null)
        {
            return new JsonWebKeySet(cached);
        }

        var json = await _httpClient.GetStringAsync(JwksUrl, cancellationToken);
        await _cache.SetAsync(JwksCacheKey, json, JwksCacheDuration, cancellationToken);

        return new JsonWebKeySet(json);
    }
}
