using FluentValidation;

namespace StudyVerse.Application.Features.CodingPractice.GetProblem;

public sealed class GetProblemQueryValidator : AbstractValidator<GetProblemQuery>
{
    public GetProblemQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProblemId).NotEmpty();
        RuleFor(x => x.LanguageId).GreaterThan(0);
    }
}
