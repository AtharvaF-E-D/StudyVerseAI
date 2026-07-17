using FluentValidation;

namespace StudyVerse.Application.Features.Tutor.GetAiUsage;

public sealed class GetAiUsageQueryValidator : AbstractValidator<GetAiUsageQuery>
{
    public GetAiUsageQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
