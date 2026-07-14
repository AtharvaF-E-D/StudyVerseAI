namespace StudyVerse.Domain.Enums;

/// <summary>
/// Purpose of a long-lived, single-use <see cref="Entities.UserToken"/> (as opposed to the
/// short-lived numeric <see cref="Entities.OtpCode"/>).
/// </summary>
public enum UserTokenPurpose
{
    EmailVerification = 0,
    PasswordReset = 1,
}
