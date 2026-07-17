using FluentValidation;

namespace StudyVerse.Application.Features.Quiz.GetQuizStats;

public sealed class GetQuizStatsQueryValidator : AbstractValidator<GetQuizStatsQuery>
{
    public GetQuizStatsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
