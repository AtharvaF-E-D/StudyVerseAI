using FluentValidation;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewSessions;

public sealed class GetInterviewSessionsQueryValidator : AbstractValidator<GetInterviewSessionsQuery>
{
    public GetInterviewSessionsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
