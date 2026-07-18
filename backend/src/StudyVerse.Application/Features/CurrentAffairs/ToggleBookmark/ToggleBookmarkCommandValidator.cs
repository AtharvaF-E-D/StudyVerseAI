using FluentValidation;

namespace StudyVerse.Application.Features.CurrentAffairs.ToggleBookmark;

public sealed class ToggleBookmarkCommandValidator : AbstractValidator<ToggleBookmarkCommand>
{
    public ToggleBookmarkCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ArticleId).NotEmpty();
    }
}
