using StudyVerse.Application.Common.Models;

namespace StudyVerse.Application.Common.Interfaces;

/// <summary>Validates a Google Sign-In ID token server-side and extracts the verified profile.</summary>
public interface IGoogleTokenValidator
{
    /// <summary>Returns null if the token is invalid, expired, or fails audience/issuer checks.</summary>
    Task<ExternalUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
