using FluentValidation;

namespace StudyVerse.Application.Features.InterviewPrep.GetResumeAnalyses;

public sealed class GetResumeAnalysesQueryValidator : AbstractValidator<GetResumeAnalysesQuery>
{
    public GetResumeAnalysesQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
