namespace StudyVerse.Domain.Enums;

/// <summary>
/// The primary signup provider for a user account. A user's credentials/social links can
/// still be extended later; this reflects only how the account was originally created.
/// </summary>
public enum AuthProvider
{
    Local = 0,
    Google = 1,
    Apple = 2,
}
