using StudyVerse.Application.Common.Models;

namespace StudyVerse.Application.Common.Interfaces;

/// <summary>Validates a Sign in with Apple identity token server-side and extracts the verified profile.</summary>
public interface IAppleTokenValidator
{
    /// <summary>Returns null if the token is invalid, expired, or fails audience/issuer/signature checks.</summary>
    Task<ExternalUserInfo?> ValidateAsync(string identityToken, CancellationToken cancellationToken = default);
}
