using FluentValidation;

namespace StudyVerse.Application.Features.Quiz.UseExtraTime;

public sealed class UseExtraTimeCommandValidator : AbstractValidator<UseExtraTimeCommand>
{
    public UseExtraTimeCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
