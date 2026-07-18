using FluentValidation;

namespace StudyVerse.Application.Features.MockTests.SubmitMockTestAttempt;

public sealed class SubmitMockTestAttemptCommandValidator : AbstractValidator<SubmitMockTestAttemptCommand>
{
    public SubmitMockTestAttemptCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AttemptId).NotEmpty();
        RuleFor(x => x.Answers).NotNull();

        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId).NotEmpty();
            answer.RuleFor(a => a.SelectedOptionIndex).InclusiveBetween(0, 3);
        });
    }
}
