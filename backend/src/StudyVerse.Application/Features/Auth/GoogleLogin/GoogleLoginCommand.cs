using MediatR;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.GoogleLogin;

public sealed record GoogleLoginCommand(string IdToken, string DeviceId, string? DeviceName)
    : IRequest<Result<AuthSessionDto>>;
