using FluentValidation;

namespace StudyVerse.Application.Features.Gamification.ClaimDailyReward;

public sealed class ClaimDailyRewardCommandValidator : AbstractValidator<ClaimDailyRewardCommand>
{
    public ClaimDailyRewardCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
