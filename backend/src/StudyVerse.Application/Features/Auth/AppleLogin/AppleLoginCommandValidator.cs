using FluentValidation;

namespace StudyVerse.Application.Features.Auth.AppleLogin;

public sealed class AppleLoginCommandValidator : AbstractValidator<AppleLoginCommand>
{
    public AppleLoginCommandValidator()
    {
        RuleFor(x => x.IdentityToken).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DeviceName).MaximumLength(200);
        RuleFor(x => x.FullName).MaximumLength(200);
    }
}
