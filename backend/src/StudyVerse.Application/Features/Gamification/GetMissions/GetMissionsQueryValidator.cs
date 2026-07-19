using FluentValidation;

namespace StudyVerse.Application.Features.Gamification.GetMissions;

public sealed class GetMissionsQueryValidator : AbstractValidator<GetMissionsQuery>
{
    public GetMissionsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
