using FluentValidation;

namespace StudyVerse.Application.Features.MockTests.GetMockTestAttempts;

public sealed class GetMockTestAttemptsQueryValidator : AbstractValidator<GetMockTestAttemptsQuery>
{
    public GetMockTestAttemptsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
