namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Sends transactional emails. Implementations are drop-in replaceable per environment (e.g. a
/// logging implementation for local dev, a real provider like SendGrid in staging/production).
/// </summary>
public interface IEmailSender
{
    Task SendEmailVerificationAsync(
        string toEmail,
        string displayName,
        string verificationLink,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(
        string toEmail,
        string displayName,
        string resetLink,
        CancellationToken cancellationToken = default);
}
