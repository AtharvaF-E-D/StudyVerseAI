using FluentValidation;

namespace StudyVerse.Application.Features.Gamification.GetSummary;

public sealed class GetGamificationSummaryQueryValidator : AbstractValidator<GetGamificationSummaryQuery>
{
    public GetGamificationSummaryQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
