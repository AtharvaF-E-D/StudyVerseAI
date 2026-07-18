namespace StudyVerse.Application.Features.CurrentAffairs.Common;

/// <summary>
/// Builds the single user-role prompt sent to <c>IAiChatProvider.GetCompletionAsync</c> for an
/// article's comprehension quiz. That interface has no way to override its fixed tutoring system
/// prompt (see <c>OpenAiChatProvider.SystemPrompt</c>) - reusing it rather than standing up a new AI
/// provider abstraction (per the phase spec) means the task instructions have to live entirely in
/// this one user-role message, the same approach <c>MockTestWeaknessPromptBuilder</c> takes.
/// </summary>
internal static class NewsQuizPromptBuilder
{
    private const int QuestionCount = 5;

    /// <summary>Guards against blowing the model's context window on an unusually long article body -
    /// same reasoning as <c>OpenAiNoteGenerationProvider.MaxSourceTextLength</c>.</summary>
    private const int MaxArticleTextLength = 6_000;

    public static string Build(string title, string articleText)
    {
        var truncatedText = articleText.Length > MaxArticleTextLength ? articleText[..MaxArticleTextLength] : articleText;

        return
            "A student just read this news article and wants to check their comprehension of it.\n\n" +
            $"Title: {title}\n" +
            $"Article text: {truncatedText}\n\n" +
            $"Write exactly {QuestionCount} multiple-choice questions that test understanding of THIS " +
            "article's actual content - do not invent facts it doesn't contain. Respond with ONLY a " +
            "JSON object (no prose, no markdown fences) with exactly this shape:\n" +
            "{ \"questions\": [ { \"questionText\": string, \"options\": [exactly 4 strings], " +
            "\"correctOptionIndex\": 0-based index into options, \"explanation\": string }, ... ] }";
    }
}
