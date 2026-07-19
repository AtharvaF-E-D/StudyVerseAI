using FluentValidation;

namespace StudyVerse.Application.Features.Gamification.Spin;

public sealed class SpinCommandValidator : AbstractValidator<SpinCommand>
{
    public SpinCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
