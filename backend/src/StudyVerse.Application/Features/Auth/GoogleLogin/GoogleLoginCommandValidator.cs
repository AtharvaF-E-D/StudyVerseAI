using FluentValidation;

namespace StudyVerse.Application.Features.Auth.GoogleLogin;

public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DeviceName).MaximumLength(200);
    }
}
