using FluentValidation;

namespace StudyVerse.Application.Features.Auth.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(200);
    }
}
