using FluentValidation;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Auth.RequestOtp;

public sealed class RequestOtpCommandValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpCommandValidator()
    {
        RuleFor(x => x.Channel).IsInEnum();
        RuleFor(x => x.Purpose).IsInEnum();

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
