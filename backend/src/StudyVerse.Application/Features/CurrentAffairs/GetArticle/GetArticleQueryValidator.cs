using FluentValidation;

namespace StudyVerse.Application.Features.CurrentAffairs.GetArticle;

public sealed class GetArticleQueryValidator : AbstractValidator<GetArticleQuery>
{
    public GetArticleQueryValidator()
    {
        RuleFor(x => x.ArticleId).NotEmpty();
    }
}
