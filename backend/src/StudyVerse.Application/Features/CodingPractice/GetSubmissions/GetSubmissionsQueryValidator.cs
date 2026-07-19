using FluentValidation;

namespace StudyVerse.Application.Features.CodingPractice.GetSubmissions;

public sealed class GetSubmissionsQueryValidator : AbstractValidator<GetSubmissionsQuery>
{
    public GetSubmissionsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
