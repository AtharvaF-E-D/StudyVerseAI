using Microsoft.Extensions.Logging;
using StudyVerse.Application.Common.Interfaces;

namespace StudyVerse.Infrastructure.Email;

/// <summary>
/// Development/default email sender: logs the email content (including the verification/reset
/// link) instead of dispatching through a real provider. This is a fully working implementation
/// for local development, not a stub — swap the DI registration for a real provider (SendGrid,
/// Postmark, SES, ...) implementing the same <see cref="IEmailSender"/> interface for
/// staging/production.
/// </summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(
        string toEmail,
        string displayName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[DEV EMAIL] Verify your email, {DisplayName} <{ToEmail}> — open this link to confirm your account: {VerificationLink}",
            displayName,
            toEmail,
            verificationLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        string toEmail,
        string displayName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[DEV EMAIL] Reset your password, {DisplayName} <{ToEmail}> — open this link to choose a new password: {ResetLink}",
            displayName,
            toEmail,
            resetLink);

        return Task.CompletedTask;
    }
}
