using FluentValidation;

namespace StudyVerse.Application.Features.CurrentAffairs.SearchArticles;

public sealed class SearchArticlesQueryValidator : AbstractValidator<SearchArticlesQuery>
{
    public SearchArticlesQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("A search query is required.")
            .MaximumLength(200);
    }
}
