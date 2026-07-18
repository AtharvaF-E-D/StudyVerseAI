using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Features.CurrentAffairs.Common;

/// <summary>
/// Upserts GNews results into <see cref="NewsArticle"/> rows, shared by <c>GetArticlesByCategoryQuery</c>
/// (category refresh) and <c>SearchArticlesQuery</c> (live search) so the "skip ones whose
/// <see cref="NewsArticle.ExternalId"/> already exists" dedupe logic lives in exactly one place.
/// </summary>
public static class NewsArticleUpsertService
{
    /// <summary>
    /// Stamped onto brand-new rows that only ever arrived via <c>SearchArticlesQuery</c> - GNews's
    /// <c>search</c> endpoint (unlike <c>top-headlines</c>) doesn't accept or return a category, so
    /// there's no real one of <c>CurrentAffairs.NewsCategories.All</c> to store. Deliberately NOT a
    /// member of that catalog, so it can never match <c>GetArticlesByCategoryQuery</c>'s category
    /// filter - a search-only article simply never surfaces via category browsing, only via search,
    /// its detail view, bookmarks, or its quiz (all looked up by <c>Id</c>, not <c>Category</c>).
    /// If a search result's <see cref="NewsArticle.ExternalId"/> already exists from a prior category
    /// fetch, that row's real category is left untouched (see <see cref="UpsertAsync"/>) - this
    /// pseudo-category is only ever assigned when inserting a brand-new row.
    /// </summary>
    public const string SearchPseudoCategory = "search";

    /// <summary>Upserts <paramref name="fetchedArticles"/>, returning the full set of corresponding
    /// <see cref="NewsArticle"/> rows (both newly-inserted and already-existing) in the same order
    /// they were fetched, so callers that need "what GNews just returned" (search) can use the
    /// return value directly, while callers that only care about the persisted cache (category
    /// refresh) can ignore it and re-query the DB themselves.</summary>
    public static async Task<IReadOnlyList<NewsArticle>> UpsertAsync(
        IAppDbContext db,
        IReadOnlyList<GNewsArticleDto> fetchedArticles,
        string category,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        if (fetchedArticles.Count == 0)
        {
            return [];
        }

        var externalIds = fetchedArticles.Select(a => a.ExternalId).ToList();
        var existingArticles = await db.NewsArticles
            .Where(a => externalIds.Contains(a.ExternalId))
            .ToListAsync(cancellationToken);
        var byExternalId = existingArticles.ToDictionary(a => a.ExternalId);

        var result = new List<NewsArticle>(fetchedArticles.Count);

        foreach (var fetched in fetchedArticles)
        {
            if (byExternalId.TryGetValue(fetched.ExternalId, out var existingArticle))
            {
                result.Add(existingArticle);
                continue;
            }

            var newArticle = new NewsArticle
            {
                Id = Guid.NewGuid(),
                ExternalId = fetched.ExternalId,
                Title = fetched.Title,
                Description = fetched.Description,
                Content = fetched.Content ?? string.Empty,
                Url = fetched.Url,
                ImageUrl = fetched.ImageUrl,
                Category = category,
                SourceName = fetched.SourceName,
                PublishedAtUtc = DateTime.SpecifyKind(fetched.PublishedAtUtc, DateTimeKind.Utc),
                FetchedAtUtc = nowUtc,
            };

            db.NewsArticles.Add(newArticle);
            // Guards against GNews returning the same ExternalId twice within one response (seen in
            // practice across overlapping top-headlines/search pages) - without this, the second
            // occurrence would fall through to "not existing" again and try to insert a duplicate.
            byExternalId[fetched.ExternalId] = newArticle;
            result.Add(newArticle);
        }

        await db.SaveChangesAsync(cancellationToken);
        return result;
    }
}
