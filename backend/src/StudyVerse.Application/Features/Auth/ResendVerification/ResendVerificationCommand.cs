using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.ResendVerification;

public sealed record ResendVerificationCommand(string Email) : IRequest<Result>;
