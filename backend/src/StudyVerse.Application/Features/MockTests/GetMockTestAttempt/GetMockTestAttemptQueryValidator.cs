using FluentValidation;

namespace StudyVerse.Application.Features.MockTests.GetMockTestAttempt;

public sealed class GetMockTestAttemptQueryValidator : AbstractValidator<GetMockTestAttemptQuery>
{
    public GetMockTestAttemptQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AttemptId).NotEmpty();
    }
}
