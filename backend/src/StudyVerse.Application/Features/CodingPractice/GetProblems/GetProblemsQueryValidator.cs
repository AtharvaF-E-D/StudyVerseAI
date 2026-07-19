using FluentValidation;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.CodingPractice.GetProblems;

public sealed class GetProblemsQueryValidator : AbstractValidator<GetProblemsQuery>
{
    public GetProblemsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Difficulty)
            .Must(d => Enum.TryParse<CodingDifficulty>(d, ignoreCase: true, out _))
            .WithMessage("Difficulty must be one of: Easy, Medium, Hard.")
            .When(x => !string.IsNullOrWhiteSpace(x.Difficulty));
    }
}
