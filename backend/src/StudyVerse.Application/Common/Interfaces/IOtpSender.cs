using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Delivers a one-time-passcode over email or phone (SMS). Implementations are drop-in
/// replaceable per environment (e.g. a logging implementation for local dev, Twilio/SNS in prod).
/// </summary>
public interface IOtpSender
{
    Task SendOtpAsync(
        OtpChannel channel,
        string destination,
        string code,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default);
}
