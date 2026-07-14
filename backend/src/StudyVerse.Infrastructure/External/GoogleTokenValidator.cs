using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Infrastructure.Options;

namespace StudyVerse.Infrastructure.External;

/// <summary>Validates a Google Sign-In ID token using Google's own client library, offline (no HTTP call per request; Google.Apis.Auth caches Google's public keys internally).</summary>
public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleOptions _options;
    private readonly ILogger<GoogleTokenValidator> _logger;

    public GoogleTokenValidator(IOptions<GoogleOptions> options, ILogger<GoogleTokenValidator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ExternalUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.ClientId],
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new ExternalUserInfo(payload.Email, payload.EmailVerified, payload.Name, payload.Subject);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Google ID token validation failed.");
            return null;
        }
    }
}
