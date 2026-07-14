using System.Security.Claims;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Common.Interfaces;

public interface IJwtTokenService
{
    /// <summary>Generates a short-lived signed JWT access token carrying the user's identity claims.</summary>
    AccessTokenResult GenerateAccessToken(User user);

    /// <summary>Generates a cryptographically random refresh token and its SHA-256 hash.</summary>
    RefreshTokenResult GenerateRefreshToken();

    /// <summary>Hashes a raw refresh token value using the same algorithm used at issuance, for DB lookups.</summary>
    string HashToken(string rawToken);

    /// <summary>
    /// Extracts the principal from an access token even if it has already expired (signature is
    /// still validated). Returns null if the token is malformed or the signature is invalid.
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}
