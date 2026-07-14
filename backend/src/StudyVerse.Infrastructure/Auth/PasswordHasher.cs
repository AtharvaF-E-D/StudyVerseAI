using Microsoft.AspNetCore.Identity;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Entities;
using AppPasswordVerificationResult = StudyVerse.Application.Common.Models.PasswordVerificationResult;
using IdentityPasswordVerificationResult = Microsoft.AspNetCore.Identity.PasswordVerificationResult;

namespace StudyVerse.Infrastructure.Auth;

/// <summary>
/// Wraps ASP.NET Core Identity's <see cref="PasswordHasher{TUser}"/> (PBKDF2, framework-provided)
/// so the Application layer never has to depend on Identity directly.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _identityHasher = new();

    public string HashPassword(User user, string password) => _identityHasher.HashPassword(user, password);

    public AppPasswordVerificationResult VerifyPassword(User user, string hashedPassword, string providedPassword)
    {
        var result = _identityHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

        return result switch
        {
            IdentityPasswordVerificationResult.Success => AppPasswordVerificationResult.Success,
            IdentityPasswordVerificationResult.SuccessRehashNeeded => AppPasswordVerificationResult.SuccessRehashNeeded,
            _ => AppPasswordVerificationResult.Failed,
        };
    }
}
