using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.GetArticlesByCategory;

/// <summary>
/// GNews's free tier has a 12-hour data delay and a modest daily request quota, so this never calls
/// <see cref="IGNewsProvider"/> on every request - only when the category's cache has gone stale by
/// more than <see cref="CacheDuration"/> (or nothing has ever been fetched for it), matching
/// <see cref="StudyVerse.Application.Common.Services.StreakService"/>'s
/// compare-a-cached-timestamp-to-now shape for freshness checks.
/// </summary>
public sealed class GetArticlesByCategoryQueryHandler : IRequestHandler<GetArticlesByCategoryQuery, Result<IReadOnlyList<NewsArticleDto>>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

    private readonly IAppDbContext _db;
    private readonly IGNewsProvider _gNewsProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetArticlesByCategoryQueryHandler(IAppDbContext db, IGNewsProvider gNewsProvider, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _gNewsProvider = gNewsProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<NewsArticleDto>>> Handle(GetArticlesByCategoryQuery request, CancellationToken cancellationToken)
    {
        // Already checked by GetArticlesByCategoryQueryValidator; normalized again here since it's
        // what both the cache-staleness lookup and the eventual upsert key off of.
        var category = request.Category.Trim().ToLowerInvariant();

        var newestFetchedAt = await _db.NewsArticles
            .Where(a => a.Category == category)
            .OrderByDescending(a => a.FetchedAtUtc)
            .Select(a => (DateTime?)a.FetchedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var isStale = newestFetchedAt is null || _dateTimeProvider.UtcNow - newestFetchedAt.Value > CacheDuration;

        if (isStale)
        {
            var fetched = await _gNewsProvider.GetTopHeadlinesAsync(category, cancellationToken);
            await NewsArticleUpsertService.UpsertAsync(_db, fetched, category, _dateTimeProvider.UtcNow, cancellationToken);
        }

        // Re-read from the DB rather than trusting the upsert's return value directly: a GNews
        // outage (empty `fetched`) or a still-fresh cache must still serve whatever's already
        // persisted from a previous successful fetch.
        var articles = await _db.NewsArticles
            .Where(a => a.Category == category)
            .OrderByDescending(a => a.PublishedAtUtc)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<NewsArticleDto>>(articles.Select(NewsArticleMappings.ToDto).ToList());
    }
}
