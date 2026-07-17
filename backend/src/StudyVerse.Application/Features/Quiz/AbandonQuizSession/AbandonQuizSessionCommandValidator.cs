using FluentValidation;

namespace StudyVerse.Application.Features.Quiz.AbandonQuizSession;

public sealed class AbandonQuizSessionCommandValidator : AbstractValidator<AbandonQuizSessionCommand>
{
    public AbandonQuizSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
