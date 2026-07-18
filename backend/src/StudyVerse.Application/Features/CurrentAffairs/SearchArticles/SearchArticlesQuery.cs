using MediatR;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.SearchArticles;

/// <summary>
/// Live search against GNews - never cache-first, since search terms are unpredictable (unlike the
/// fixed 9-category feed, there's no meaningful "is this query's cache stale" check to run).
///
/// Named a Query, not a Command, even though it upserts DB rows as a side effect: from the caller's
/// perspective this is a read operation (search results), and <c>GetArticlesByCategoryQuery</c> -
/// already a Query - does the exact same "fetch external data, cache it, return it" thing, so this
/// stays consistent with that existing precedent rather than introducing a naming split between two
/// handlers that behave identically.
/// </summary>
public sealed record SearchArticlesQuery(string Query) : IRequest<Result<IReadOnlyList<NewsArticleDto>>>;
