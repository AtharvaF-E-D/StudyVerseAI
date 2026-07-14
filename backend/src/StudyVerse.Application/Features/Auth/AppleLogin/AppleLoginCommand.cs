using MediatR;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.AppleLogin;

public sealed record AppleLoginCommand(string IdentityToken, string DeviceId, string? DeviceName, string? FullName)
    : IRequest<Result<AuthSessionDto>>;
