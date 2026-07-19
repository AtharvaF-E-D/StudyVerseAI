using FluentValidation;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewStats;

public sealed class GetInterviewStatsQueryValidator : AbstractValidator<GetInterviewStatsQuery>
{
    public GetInterviewStatsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
