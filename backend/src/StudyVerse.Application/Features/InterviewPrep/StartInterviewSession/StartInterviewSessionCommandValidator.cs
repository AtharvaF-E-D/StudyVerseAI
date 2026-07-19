using FluentValidation;

namespace StudyVerse.Application.Features.InterviewPrep.StartInterviewSession;

public sealed class StartInterviewSessionCommandValidator : AbstractValidator<StartInterviewSessionCommand>
{
    public StartInterviewSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
    }
}
