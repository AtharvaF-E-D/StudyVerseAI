using FluentValidation;

namespace StudyVerse.Application.Features.CurrentAffairs.GetBookmarks;

public sealed class GetBookmarksQueryValidator : AbstractValidator<GetBookmarksQuery>
{
    public GetBookmarksQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
