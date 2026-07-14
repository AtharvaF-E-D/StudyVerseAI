using MediatR;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.VerifyOtp;

public sealed record VerifyOtpCommand(
    OtpChannel Channel,
    string Destination,
    string Code,
    OtpPurpose Purpose,
    string DeviceId,
    string? DeviceName) : IRequest<Result<AuthSessionDto>>;
