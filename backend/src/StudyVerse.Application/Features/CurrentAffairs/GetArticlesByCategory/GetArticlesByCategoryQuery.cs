using MediatR;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.GetArticlesByCategory;

/// <summary>
/// The "daily news summaries" surface for one category. Cache-first: if the newest cached article
/// for <paramref name="Category"/> was fetched more than 6 hours ago (or none exist yet), one
/// <c>IGNewsProvider.GetTopHeadlinesAsync</c> call refreshes the cache before the freshest
/// <paramref name="Take"/> articles are read back out of the DB - see the handler's doc comment for
/// why GNews's free-tier 12-hour delay and modest quota make that refresh interval the right one.
/// </summary>
public sealed record GetArticlesByCategoryQuery(string Category, int Take) : IRequest<Result<IReadOnlyList<NewsArticleDto>>>;
