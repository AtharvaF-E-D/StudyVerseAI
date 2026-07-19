using FluentValidation;

namespace StudyVerse.Application.Features.InterviewPrep.SubmitAnswer;

public sealed class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    /// <summary>Generous enough for a real spoken-length answer transcribed to text, small enough to
    /// keep the AI grading prompt bounded — same reasoning as
    /// <c>InterviewAnswerGradingPromptBuilder.MaxAnswerTextLength</c>, just enforced earlier.</summary>
    private const int MaxAnswerTextLength = 4_000;

    public SubmitAnswerCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.QuestionId).NotEmpty();

        RuleFor(x => x.AnswerText)
            .NotEmpty().WithMessage("An answer is required.")
            .MaximumLength(MaxAnswerTextLength);
    }
}
