using MediatR;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password, string DeviceId, string? DeviceName)
    : IRequest<Result<AuthSessionDto>>;
