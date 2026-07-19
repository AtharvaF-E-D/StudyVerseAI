using FluentValidation;

namespace StudyVerse.Application.Features.Gamification.GetBadges;

public sealed class GetBadgesQueryValidator : AbstractValidator<GetBadgesQuery>
{
    public GetBadgesQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
