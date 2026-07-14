using StudyVerse.Domain.Enums;

namespace StudyVerse.Api.Contracts;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);

public sealed record LoginRequest(string Email, string Password, string DeviceId, string? DeviceName);

public sealed record RefreshRequest(string RefreshToken, string DeviceId);

public sealed record LogoutRequest(string RefreshToken, string DeviceId);

public sealed record VerifyEmailRequest(Guid UserId, string Token);

public sealed record ResendVerificationRequest(string Email);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);

public sealed record OtpRequestRequest(OtpChannel Channel, string Destination, OtpPurpose Purpose);

public sealed record OtpVerifyRequest(
    OtpChannel Channel,
    string Destination,
    string Code,
    OtpPurpose Purpose,
    string DeviceId,
    string? DeviceName);

public sealed record GoogleLoginRequest(string IdToken, string DeviceId, string? DeviceName);

public sealed record AppleLoginRequest(string IdentityToken, string DeviceId, string? DeviceName, string? FullName);
