using MediatR;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken, string DeviceId) : IRequest<Result<TokenPairDto>>;
