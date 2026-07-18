using FluentValidation;

namespace StudyVerse.Application.Features.StudyPlanner.ArchiveStudyPlan;

public sealed class ArchiveStudyPlanCommandValidator : AbstractValidator<ArchiveStudyPlanCommand>
{
    public ArchiveStudyPlanCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PlanId).NotEmpty();
    }
}
