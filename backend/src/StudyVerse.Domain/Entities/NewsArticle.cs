namespace StudyVerse.Domain.Entities;

/// <summary>
/// One news article cached from GNews. <see cref="ExternalId"/> is GNews's own article <c>id</c> —
/// unique-indexed so refreshing a category (<c>GetArticlesByCategoryQuery</c>) or running a search
/// (<c>SearchArticlesQuery</c>) can upsert without ever inserting the same story twice, even though
/// GNews returns overlapping results across separate calls. <see cref="Category"/> is one of
/// <c>CurrentAffairs.NewsCategories.All</c> for articles fetched via the category feed, or
/// <c>NewsArticleUpsertService.SearchPseudoCategory</c> for articles that only ever came in via
/// search (see that constant's doc comment) — never used for cache-staleness checks in that case.
/// <see cref="Description"/> is shown as-is as the "daily summary" surface: GNews's own description,
/// not an AI-generated one — see <c>GetArticlesByCategoryQuery</c>'s doc comment for why fabricating
/// a per-article AI summary on top of a source that already provides one would be dishonest.
/// <see cref="FetchedAtUtc"/> (when WE cached it) is distinct from <see cref="PublishedAtUtc"/>
/// (when the story was published) and is what cache-staleness checks compare against.
/// </summary>
public class NewsArticle
{
    public Guid Id { get; set; }

    public string ExternalId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Content { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public string Category { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public DateTime PublishedAtUtc { get; set; }

    public DateTime FetchedAtUtc { get; set; }
}
