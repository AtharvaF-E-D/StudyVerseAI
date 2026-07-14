using MediatR;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.RequestOtp;

public sealed record RequestOtpCommand(OtpChannel Channel, string Destination, OtpPurpose Purpose) : IRequest<Result>;
