using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.Register;

public sealed record RegisterCommand(string Email, string Password, string DisplayName)
    : IRequest<Result<RegisterResponseDto>>;

public sealed record RegisterResponseDto(Guid UserId, string Email, string Message);
