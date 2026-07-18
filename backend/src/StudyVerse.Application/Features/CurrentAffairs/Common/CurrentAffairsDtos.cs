namespace StudyVerse.Application.Features.CurrentAffairs.Common;

/// <summary>One cached news article, used identically for the category feed, search results,
/// article detail, and bookmarks list. <see cref="Description"/> is GNews's own description field -
/// the "daily summary" surface for this feature (see <c>NewsArticle</c>'s doc comment for why that's
/// the honest choice over fabricating a per-article AI summary).</summary>
public sealed record NewsArticleDto(
    Guid Id,
    string Title,
    string? Description,
    string Content,
    string Url,
    string? ImageUrl,
    string Category,
    string SourceName,
    DateTime PublishedAtUtc);

/// <summary>Result of <c>ToggleBookmarkCommand</c> - the new state, so the client doesn't need a
/// separate re-fetch to know whether the toggle turned the bookmark on or off.</summary>
public sealed record ToggleBookmarkResultDto(Guid ArticleId, bool IsBookmarked);

/// <summary>One question in an article's cached comprehension quiz (<c>GetArticleQuizQuery</c>).
///
/// <see cref="CorrectOptionIndex"/> is deliberately included here, unlike Rapid Fire Quiz's
/// <c>QuizSessionQuestion</c> DTOs, which withhold it until after submission: there is no "session"
/// being played for a news comprehension quiz, no score, no leaderboard, and no opponent to cheat
/// against - it's shown to one reader with immediate per-question feedback. Withholding the answer
/// here would only add a pointless extra round trip with no cheating vector it actually closes.
/// </summary>
public sealed record NewsArticleQuizQuestionDto(string QuestionText, IReadOnlyList<string> Options, int CorrectOptionIndex, string Explanation);

public sealed record NewsArticleQuizDto(Guid ArticleId, IReadOnlyList<NewsArticleQuizQuestionDto> Questions, DateTime GeneratedAtUtc);

/// <summary>The shared, once-per-ISO-week AI digest (<c>GetWeeklyDigestQuery</c>).</summary>
public sealed record WeeklyDigestDto(DateOnly WeekStartDateUtc, string SummaryText, DateTime GeneratedAtUtc);
