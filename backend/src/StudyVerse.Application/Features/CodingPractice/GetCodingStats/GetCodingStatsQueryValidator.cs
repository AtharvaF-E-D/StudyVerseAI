using FluentValidation;

namespace StudyVerse.Application.Features.CodingPractice.GetCodingStats;

public sealed class GetCodingStatsQueryValidator : AbstractValidator<GetCodingStatsQuery>
{
    public GetCodingStatsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
