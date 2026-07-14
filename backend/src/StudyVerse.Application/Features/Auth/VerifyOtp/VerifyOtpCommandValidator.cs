using FluentValidation;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.VerifyOtp;

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Channel).IsInEnum();
        RuleFor(x => x.Purpose).IsInEnum();
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches(@"^\d{6}$");
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DeviceName).MaximumLength(200);

        RuleFor(x => x.Destination)
            .NotEmpty()
            .EmailAddress()
            .When(x => x.Channel == OtpChannel.Email);

        RuleFor(x => x.Destination)
            .NotEmpty()
            .Matches(@"^\+?[1-9]\d{6,14}$")
            .WithMessage("Phone number must be in E.164 format, e.g. +15551234567.")
            .When(x => x.Channel == OtpChannel.Phone);
    }
}
