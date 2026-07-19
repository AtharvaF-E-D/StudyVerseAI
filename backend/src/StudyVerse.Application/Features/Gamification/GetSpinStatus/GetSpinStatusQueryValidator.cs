using FluentValidation;

namespace StudyVerse.Application.Features.Gamification.GetSpinStatus;

public sealed class GetSpinStatusQueryValidator : AbstractValidator<GetSpinStatusQuery>
{
    public GetSpinStatusQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
