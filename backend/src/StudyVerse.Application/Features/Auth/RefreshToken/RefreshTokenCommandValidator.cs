using FluentValidation;

namespace StudyVerse.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(200);
    }
}
