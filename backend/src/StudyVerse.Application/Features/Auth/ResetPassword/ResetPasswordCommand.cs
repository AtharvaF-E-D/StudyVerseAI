using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Result>;
