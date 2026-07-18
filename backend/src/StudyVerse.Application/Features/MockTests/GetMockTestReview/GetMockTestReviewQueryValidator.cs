using FluentValidation;

namespace StudyVerse.Application.Features.MockTests.GetMockTestReview;

public sealed class GetMockTestReviewQueryValidator : AbstractValidator<GetMockTestReviewQuery>
{
    public GetMockTestReviewQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AttemptId).NotEmpty();
    }
}
