namespace StudyVerse.Application.Common.Models;

/// <summary>
/// Application-owned mirror of <c>Microsoft.AspNetCore.Identity.PasswordVerificationResult</c>
/// so the Application layer does not need a dependency on ASP.NET Core Identity.
/// </summary>
public enum PasswordVerificationResult
{
    Failed = 0,
    Success = 1,

    /// <summary>Password is correct, but the hash should be regenerated (e.g. weaker legacy algorithm/iteration count).</summary>
    SuccessRehashNeeded = 2,
}
