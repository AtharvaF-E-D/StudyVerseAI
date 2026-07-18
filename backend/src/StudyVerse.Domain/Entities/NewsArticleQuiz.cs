namespace StudyVerse.Domain.Entities;

/// <summary>
/// A cached, AI-generated comprehension quiz for one <see cref="NewsArticle"/> — unique on
/// <see cref="ArticleId"/> so re-viewing an article's quiz is a DB read instead of a repeat OpenAI
/// call (see <c>GetArticleQuizQuery</c>'s handler). <see cref="QuestionsJson"/> stores an array of
/// <c>{ questionText, options: string[4], correctOptionIndex, explanation }</c>, the same
/// JSON-text-column philosophy <see cref="NoteContent"/> uses for its seven generated pieces.
///
/// Unlike Rapid Fire Quiz's <c>QuizSessionQuestion</c>, there's no "session" here to protect from
/// cheating — this is a single-shot comprehension check shown with immediate per-question feedback,
/// not a scored/competitive attempt — so <c>correctOptionIndex</c> is deliberately included in the
/// DTO returned to the client rather than withheld. See <c>GetArticleQuizQuery</c>'s DTO doc comment
/// for the full reasoning.
/// </summary>
public class NewsArticleQuiz
{
    public Guid Id { get; set; }

    public Guid ArticleId { get; set; }

    public string QuestionsJson { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; }

    public NewsArticle? Article { get; set; }
}
