using FluentValidation;
using StudyVerse.Domain.CurrentAffairs;

namespace StudyVerse.Application.Features.CurrentAffairs.GetArticlesByCategory;

public sealed class GetArticlesByCategoryQueryValidator : AbstractValidator<GetArticlesByCategoryQuery>
{
    public GetArticlesByCategoryQueryValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(category => NewsCategories.All.Contains(category.Trim().ToLowerInvariant()))
            .WithMessage($"Category must be one of: {string.Join(", ", NewsCategories.All)}.");

        RuleFor(x => x.Take).GreaterThan(0);
    }
}
