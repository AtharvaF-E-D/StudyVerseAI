using FluentValidation;

namespace StudyVerse.Application.Features.Quiz.GetDailyQuizChallengeStatus;

public sealed class GetDailyQuizChallengeStatusQueryValidator : AbstractValidator<GetDailyQuizChallengeStatusQuery>
{
    public GetDailyQuizChallengeStatusQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
