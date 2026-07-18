using FluentValidation;

namespace StudyVerse.Application.Features.StudyPlanner.GetActivePlan;

public sealed class GetActivePlanQueryValidator : AbstractValidator<GetActivePlanQuery>
{
    public GetActivePlanQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
