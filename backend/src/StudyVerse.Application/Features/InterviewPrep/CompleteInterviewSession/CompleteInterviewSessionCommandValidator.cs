using FluentValidation;

namespace StudyVerse.Application.Features.InterviewPrep.CompleteInterviewSession;

public sealed class CompleteInterviewSessionCommandValidator : AbstractValidator<CompleteInterviewSessionCommand>
{
    public CompleteInterviewSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
