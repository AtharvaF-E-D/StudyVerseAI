using Microsoft.Extensions.Logging;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Infrastructure.Otp;

/// <summary>
/// Development/default OTP sender: logs the one-time code instead of dispatching a real SMS/email
/// through a provider. A fully working implementation for local development — swap the DI
/// registration for a real provider (Twilio, SNS, ...) implementing <see cref="IOtpSender"/> for
/// staging/production.
/// </summary>
public sealed class LoggingOtpSender : IOtpSender
{
    private readonly ILogger<LoggingOtpSender> _logger;

    public LoggingOtpSender(ILogger<LoggingOtpSender> logger)
    {
        _logger = logger;
    }

    public Task SendOtpAsync(
        OtpChannel channel,
        string destination,
        string code,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[DEV OTP] {Channel} OTP for {Destination} ({Purpose}): {Code} (expires in 5 minutes)",
            channel,
            destination,
            purpose,
            code);

        return Task.CompletedTask;
    }
}
