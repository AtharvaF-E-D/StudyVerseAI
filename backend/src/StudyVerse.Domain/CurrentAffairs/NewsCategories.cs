namespace StudyVerse.Domain.CurrentAffairs;

/// <summary>
/// The fixed set of categories the Current Affairs feed browses via GNews's <c>top-headlines</c>
/// endpoint (these exact lowercase values are what GNews itself expects as the <c>category</c>
/// query parameter). Handlers validate a requested category against this list rather than trusting
/// arbitrary client input — the same "in-code catalog is the source of truth" idea as
/// <c>Quiz.QuizCategories</c>.
///
/// Search results (<c>SearchArticlesQuery</c>) don't come back tagged with one of these categories
/// at all — see <c>NewsArticleUpsertService.SearchPseudoCategory</c> for how those are stored.
/// </summary>
public static class NewsCategories
{
    public const string General = "general";
    public const string World = "world";
    public const string Nation = "nation";
    public const string Business = "business";
    public const string Technology = "technology";
    public const string Entertainment = "entertainment";
    public const string Sports = "sports";
    public const string Science = "science";
    public const string Health = "health";

    public static readonly IReadOnlyList<string> All =
    [
        General,
        World,
        Nation,
        Business,
        Technology,
        Entertainment,
        Sports,
        Science,
        Health,
    ];
}
