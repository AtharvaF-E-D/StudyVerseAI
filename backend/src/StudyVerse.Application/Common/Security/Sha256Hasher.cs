using System.Security.Cryptography;
using System.Text;

namespace StudyVerse.Application.Common.Security;

/// <summary>
/// Deterministic SHA-256 hashing for values that must be looked up by hash (refresh tokens,
/// verification/reset tokens, OTP codes). Not for passwords — those go through
/// <see cref="Interfaces.IPasswordHasher"/>, which is salted and slow by design.
/// </summary>
public static class Sha256Hasher
{
    public static string Hash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
