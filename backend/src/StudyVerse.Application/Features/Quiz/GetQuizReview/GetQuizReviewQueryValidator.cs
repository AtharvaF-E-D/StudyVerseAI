using FluentValidation;

namespace StudyVerse.Application.Features.Quiz.GetQuizReview;

public sealed class GetQuizReviewQueryValidator : AbstractValidator<GetQuizReviewQuery>
{
    public GetQuizReviewQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
