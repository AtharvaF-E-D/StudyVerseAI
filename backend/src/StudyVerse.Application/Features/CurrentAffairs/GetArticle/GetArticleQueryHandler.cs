using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.GetArticle;

public sealed class GetArticleQueryHandler : IRequestHandler<GetArticleQuery, Result<NewsArticleDto>>
{
    private readonly IAppDbContext _db;

    public GetArticleQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<NewsArticleDto>> Handle(GetArticleQuery request, CancellationToken cancellationToken)
    {
        var article = await _db.NewsArticles.FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (article is null)
        {
            return Result.Failure<NewsArticleDto>("Article not found.", ResultErrorType.NotFound);
        }

        return Result.Success(NewsArticleMappings.ToDto(article));
    }
}
