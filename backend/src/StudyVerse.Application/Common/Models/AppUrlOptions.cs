namespace StudyVerse.Application.Common.Models;

/// <summary>
/// Bound from the "AppUrls" configuration section. Templates used to build the links embedded
/// in verification/reset emails. <c>{userId}</c>, <c>{email}</c> and <c>{token}</c> placeholders
/// are substituted (token/email are URL-encoded) before sending.
/// </summary>
public sealed class AppUrlOptions
{
    public const string SectionName = "AppUrls";

    public string EmailVerificationUrlTemplate { get; set; } =
        "https://app.studyverse.ai/verify-email?userId={userId}&token={token}";

    public string PasswordResetUrlTemplate { get; set; } =
        "https://app.studyverse.ai/reset-password?email={email}&token={token}";
}
