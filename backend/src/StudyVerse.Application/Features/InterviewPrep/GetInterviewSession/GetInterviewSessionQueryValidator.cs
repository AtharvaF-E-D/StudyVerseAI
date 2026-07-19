using FluentValidation;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewSession;

public sealed class GetInterviewSessionQueryValidator : AbstractValidator<GetInterviewSessionQuery>
{
    public GetInterviewSessionQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
