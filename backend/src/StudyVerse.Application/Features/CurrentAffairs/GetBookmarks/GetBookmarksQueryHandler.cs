using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.GetBookmarks;

public sealed class GetBookmarksQueryHandler : IRequestHandler<GetBookmarksQuery, Result<IReadOnlyList<NewsArticleDto>>>
{
    private readonly IAppDbContext _db;

    public GetBookmarksQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<NewsArticleDto>>> Handle(GetBookmarksQuery request, CancellationToken cancellationToken)
    {
        var articles = await (
            from bookmark in _db.NewsBookmarks
            join article in _db.NewsArticles on bookmark.ArticleId equals article.Id
            where bookmark.UserId == request.UserId
            orderby bookmark.CreatedAtUtc descending
            select article
        ).ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<NewsArticleDto>>(articles.Select(NewsArticleMappings.ToDto).ToList());
    }
}
