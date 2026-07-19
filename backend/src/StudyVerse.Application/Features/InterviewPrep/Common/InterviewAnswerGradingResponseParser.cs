using System.Text.Json;

namespace StudyVerse.Application.Features.InterviewPrep.Common;

/// <summary>One graded answer parsed out of the AI grading response — also the exact values
/// persisted onto <c>InterviewAnswer.AiScore</c>/<c>AiFeedback</c>.</summary>
public sealed record GradedAnswer(int Score, string Feedback);

/// <summary>
/// Parses the raw JSON returned by <c>IAiChatProvider.GetCompletionAsync(..., requireJsonObjectResponse: true)</c>
/// for one interview answer's real-time grading — the same tolerant shape
/// <c>NewsArticleQuizResponseParser</c>/<c>StudyPlanAiResponseParser</c> use for their own AI JSON
/// responses. A parse failure never fails the whole request (the candidate already waited for the
/// AI round trip and deserves an answer) — it falls back to a safe, honest "couldn't grade this"
/// result instead of guessing a score.
/// </summary>
public static class InterviewAnswerGradingResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private const string FallbackFeedback =
        "We couldn't automatically grade this answer — please try submitting it again.";

    public static GradedAnswer Parse(string rawJson)
    {
        RawGradedAnswer? raw;
        try
        {
            raw = JsonSerializer.Deserialize<RawGradedAnswer>(rawJson, JsonOptions);
        }
        catch (JsonException)
        {
            return new GradedAnswer(0, FallbackFeedback);
        }

        if (raw is null)
        {
            return new GradedAnswer(0, FallbackFeedback);
        }

        var score = Math.Clamp(raw.Score, 0, 10);
        var feedback = string.IsNullOrWhiteSpace(raw.Feedback) ? FallbackFeedback : raw.Feedback.Trim();

        return new GradedAnswer(score, feedback);
    }

    private sealed class RawGradedAnswer
    {
        public int Score { get; set; }

        public string? Feedback { get; set; }
    }
}
