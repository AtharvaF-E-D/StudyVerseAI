using FluentValidation;

namespace StudyVerse.Application.Features.CurrentAffairs.GetArticleQuiz;

public sealed class GetArticleQuizQueryValidator : AbstractValidator<GetArticleQuizQuery>
{
    public GetArticleQuizQueryValidator()
    {
        RuleFor(x => x.ArticleId).NotEmpty();
    }
}
