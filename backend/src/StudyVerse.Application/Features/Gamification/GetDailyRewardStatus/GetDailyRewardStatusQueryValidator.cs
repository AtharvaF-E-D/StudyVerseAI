using FluentValidation;

namespace StudyVerse.Application.Features.Gamification.GetDailyRewardStatus;

public sealed class GetDailyRewardStatusQueryValidator : AbstractValidator<GetDailyRewardStatusQuery>
{
    public GetDailyRewardStatusQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
