using System.Security.Cryptography;

namespace StudyVerse.Application.Common.Security;

/// <summary>
/// Cryptographically secure random value generation. Pure and stateless, so — unlike I/O-bound
/// concerns — it is used directly rather than hidden behind a DI-injected interface.
/// </summary>
public static class SecureTokenGenerator
{
    /// <summary>Generates a URL-safe opaque token (email verification / password reset links, etc.).</summary>
    public static string GenerateUrlSafeToken(int byteLength = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>Generates a fixed-length numeric one-time passcode (e.g. "482913").</summary>
    public static string GenerateNumericCode(int digits = 6)
    {
        Span<byte> buffer = stackalloc byte[digits];
        RandomNumberGenerator.Fill(buffer);

        var chars = new char[digits];
        for (var i = 0; i < digits; i++)
        {
            chars[i] = (char)('0' + buffer[i] % 10);
        }

        return new string(chars);
    }
}
