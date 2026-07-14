using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.VerifyEmail;

public sealed record VerifyEmailCommand(Guid UserId, string Token) : IRequest<Result>;
