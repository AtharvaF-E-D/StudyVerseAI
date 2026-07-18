using System.Text.Json;

namespace StudyVerse.Application.Features.CurrentAffairs.Common;

/// <summary>One question parsed out of the AI quiz-generation response for <c>GetArticleQuizQuery</c>,
/// also the exact shape persisted (serialized) into <c>NewsArticleQuiz.QuestionsJson</c>.</summary>
public sealed record GeneratedQuizQuestion(string QuestionText, IReadOnlyList<string> Options, int CorrectOptionIndex, string Explanation);

/// <summary>
/// Parses the raw JSON returned by <c>IAiChatProvider.GetCompletionAsync(..., requireJsonObjectResponse: true)</c>
/// for a news article's comprehension quiz - the same tolerant shape <c>NoteAiResponseMapper</c>/
/// <c>StudyPlanAiResponseParser</c> use for their own AI JSON responses: nullable raw DTOs so a
/// missing field defaults or the entry is dropped, rather than the whole parse throwing.
/// </summary>
public static class NewsArticleQuizResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<GeneratedQuizQuestion> Parse(string rawJson)
    {
        RawQuizResponse? raw;
        try
        {
            raw = JsonSerializer.Deserialize<RawQuizResponse>(rawJson, JsonOptions);
        }
        catch (JsonException)
        {
            return [];
        }

        if (raw?.Questions is null)
        {
            return [];
        }

        var results = new List<GeneratedQuizQuestion>();
        foreach (var question in raw.Questions)
        {
            if (string.IsNullOrWhiteSpace(question.QuestionText))
            {
                continue;
            }

            results.Add(new GeneratedQuizQuestion(
                question.QuestionText.Trim(),
                NormalizeOptions(question.Options),
                Math.Clamp(question.CorrectOptionIndex, 0, 3),
                question.Explanation ?? string.Empty));
        }

        return results;
    }

    /// <summary>Pads/truncates to exactly 4 options so the client can always render A-D - same
    /// reasoning as <c>NoteAiResponseMapper</c>'s identical helper.</summary>
    private static IReadOnlyList<string> NormalizeOptions(List<string>? options)
    {
        var list = options ?? [];
        if (list.Count == 4)
        {
            return list;
        }

        var normalized = new List<string>(list.Take(4));
        while (normalized.Count < 4)
        {
            normalized.Add(string.Empty);
        }

        return normalized;
    }

    private sealed class RawQuizResponse
    {
        public List<RawQuizQuestion>? Questions { get; set; }
    }

    private sealed class RawQuizQuestion
    {
        public string? QuestionText { get; set; }

        public List<string>? Options { get; set; }

        public int CorrectOptionIndex { get; set; }

        public string? Explanation { get; set; }
    }
}
