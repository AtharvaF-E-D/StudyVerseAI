using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;
