using FluentValidation;

namespace StudyVerse.Application.Features.Leaderboard.GetLeaderboard;

public sealed class GetLeaderboardQueryValidator : AbstractValidator<GetLeaderboardQuery>
{
    public GetLeaderboardQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Take).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
