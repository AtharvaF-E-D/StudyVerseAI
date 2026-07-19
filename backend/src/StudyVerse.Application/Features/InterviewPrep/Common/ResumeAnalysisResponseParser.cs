using System.Text.Json;

namespace StudyVerse.Application.Features.InterviewPrep.Common;

/// <summary>The parsed AI resume analysis, also the exact values persisted onto
/// <c>ResumeAnalysis</c>'s <c>OverallScore</c>/<c>*Json</c> columns.</summary>
public sealed record ParsedResumeAnalysis(
    int OverallScore,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<string> Suggestions);

/// <summary>
/// Parses the raw JSON returned by <c>IAiChatProvider.GetCompletionAsync(..., requireJsonObjectResponse: true)</c>
/// for resume analysis — the same tolerant shape <c>NewsArticleQuizResponseParser</c>/
/// <c>StudyPlanAiResponseParser</c> use for their own AI JSON responses. Returns null when nothing
/// usable came back at all, letting <c>UploadResumeCommandHandler</c> fail the request rather than
/// persist an empty analysis.
/// </summary>
public static class ResumeAnalysisResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Caps each list at 5 items even if the model returns more — matches the "3 to 5" the
    /// prompt asks for without trusting it blindly.</summary>
    private const int MaxListItems = 5;

    public static ParsedResumeAnalysis? Parse(string rawJson)
    {
        RawResumeAnalysis? raw;
        try
        {
            raw = JsonSerializer.Deserialize<RawResumeAnalysis>(rawJson, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (raw is null)
        {
            return null;
        }

        var strengths = Normalize(raw.Strengths);
        var weaknesses = Normalize(raw.Weaknesses);
        var suggestions = Normalize(raw.Suggestions);

        if (strengths.Count == 0 && weaknesses.Count == 0 && suggestions.Count == 0)
        {
            return null;
        }

        return new ParsedResumeAnalysis(Math.Clamp(raw.OverallScore, 0, 100), strengths, weaknesses, suggestions);
    }

    private static IReadOnlyList<string> Normalize(List<string>? items) =>
        (items ?? [])
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .Take(MaxListItems)
            .ToList();

    private sealed class RawResumeAnalysis
    {
        public int OverallScore { get; set; }

        public List<string>? Strengths { get; set; }

        public List<string>? Weaknesses { get; set; }

        public List<string>? Suggestions { get; set; }
    }
}
