using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken, string DeviceId) : IRequest<Result>;
