using MediatR;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.SearchArticles;

public sealed class SearchArticlesQueryHandler : IRequestHandler<SearchArticlesQuery, Result<IReadOnlyList<NewsArticleDto>>>
{
    private readonly IAppDbContext _db;
    private readonly IGNewsProvider _gNewsProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SearchArticlesQueryHandler(IAppDbContext db, IGNewsProvider gNewsProvider, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _gNewsProvider = gNewsProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<NewsArticleDto>>> Handle(SearchArticlesQuery request, CancellationToken cancellationToken)
    {
        var fetched = await _gNewsProvider.SearchAsync(request.Query, cancellationToken);

        var upserted = await NewsArticleUpsertService.UpsertAsync(
            _db, fetched, NewsArticleUpsertService.SearchPseudoCategory, _dateTimeProvider.UtcNow, cancellationToken);

        var ordered = upserted.OrderByDescending(a => a.PublishedAtUtc).ToList();

        return Result.Success<IReadOnlyList<NewsArticleDto>>(ordered.Select(NewsArticleMappings.ToDto).ToList());
    }
}
