using FluentValidation;

namespace StudyVerse.Application.Features.Quiz.SubmitAnswer;

public sealed class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.SelectedOptionIndex).InclusiveBetween(0, 3);
        RuleFor(x => x.TimeTakenMs).GreaterThanOrEqualTo(0).When(x => x.TimeTakenMs.HasValue);
    }
}
