namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the GNews REST API (https://gnews.io/api/v4/), so the Application layer never
/// references <c>HttpClient</c>/GNews's response shape directly (mirrors the
/// <see cref="IAiChatProvider"/>/<c>OpenAiChatProvider</c> split: interface here, provider-specific
/// implementation in Infrastructure).
///
/// Both methods are best-effort: a GNews outage, rate-limit response, or malformed payload is
/// logged and swallowed by the implementation, which returns an empty list rather than throwing -
/// callers (the cache-first query handlers) already fall back to whatever's in the DB from a
/// previous successful fetch, so a transient GNews failure should never 500 the whole request.
/// </summary>
public interface IGNewsProvider
{
    /// <param name="category">One of <c>CurrentAffairs.NewsCategories.All</c> - GNews's own
    /// <c>top-headlines?category=</c> values.</param>
    Task<IReadOnlyList<GNewsArticleDto>> GetTopHeadlinesAsync(string category, CancellationToken cancellationToken = default);

    /// <param name="query">Free-text search term, passed straight through to GNews's
    /// <c>search?q=</c> endpoint.</param>
    Task<IReadOnlyList<GNewsArticleDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
}

/// <summary>One article as returned by GNews, trimmed to the fields the Current Affairs feature
/// actually persists (<see cref="ExternalId"/> is GNews's own <c>id</c>).</summary>
public sealed record GNewsArticleDto(
    string ExternalId,
    string Title,
    string? Description,
    string? Content,
    string Url,
    string? ImageUrl,
    string SourceName,
    DateTime PublishedAtUtc);
