namespace StudyVerse.Domain.Entities;

/// <summary>
/// One user's bookmark of a <see cref="NewsArticle"/>. Unique on <c>(UserId, ArticleId)</c> so
/// <c>ToggleBookmarkCommand</c> can treat "does a row already exist" as the sole source of truth for
/// the current bookmarked state — no separate boolean flag to keep in sync.
/// </summary>
public class NewsBookmark
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ArticleId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public User? User { get; set; }

    public NewsArticle? Article { get; set; }
}
