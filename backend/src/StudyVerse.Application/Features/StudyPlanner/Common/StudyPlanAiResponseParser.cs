using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudyVerse.Application.Features.StudyPlanner.Common;

/// <summary>One task entry parsed out of the AI plan-generation response, already validated to fall
/// within the requested date range.</summary>
public sealed record GeneratedStudyTask(DateOnly Date, string Subject, string Topic, int DurationMinutes, bool IsWeakTopic);

/// <summary>
/// Parses the raw JSON returned by <c>IAiChatProvider.GetCompletionAsync(..., requireJsonObjectResponse: true)</c>
/// for the Study Planner's AI plan generation, the same tolerant way <c>NoteAiResponseMapper</c>/
/// <c>OpenAiFlashcardGenerationProvider</c> parse their own AI JSON responses (nullable raw DTOs so
/// a missing field defaults rather than throws).
///
/// Models occasionally drift on date math (e.g. returning a date a day or two outside the requested
/// range, or from the wrong year), so every parsed entry's date is checked against
/// <paramref name="minDate"/>/<paramref name="maxDate"/> and entries outside that range are FILTERED
/// OUT entirely — not clamped to the nearest boundary date. Clamping would silently pile every
/// drifted entry onto the same boundary day, which could blow that single day's minute budget far
/// past what <c>CreateStudyPlanCommandHandler</c> asked for; dropping the (hopefully rare) bad
/// entries is safer and keeps every persisted task's date trustworthy.
/// </summary>
public static class StudyPlanAiResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Sanity bounds for a single session's length - guards against a hallucinated duration
    /// (e.g. zero, negative, or absurdly large) rather than trusting the model's number outright.</summary>
    private const int MinDurationMinutes = 5;
    private const int MaxDurationMinutes = 480;

    public static IReadOnlyList<GeneratedStudyTask> Parse(string rawJson, DateOnly minDate, DateOnly maxDate)
    {
        RawStudyPlanResponse? raw;
        try
        {
            raw = JsonSerializer.Deserialize<RawStudyPlanResponse>(rawJson, JsonOptions);
        }
        catch (JsonException)
        {
            return [];
        }

        if (raw?.Tasks is null)
        {
            return [];
        }

        var results = new List<GeneratedStudyTask>();

        foreach (var task in raw.Tasks)
        {
            if (!DateOnly.TryParse(task.Date, out var date))
            {
                continue;
            }

            if (date < minDate || date > maxDate)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(task.Subject) || string.IsNullOrWhiteSpace(task.Topic))
            {
                continue;
            }

            if (task.DurationMinutes <= 0)
            {
                continue;
            }

            var duration = Math.Clamp(task.DurationMinutes, MinDurationMinutes, MaxDurationMinutes);

            results.Add(new GeneratedStudyTask(date, task.Subject.Trim(), task.Topic.Trim(), duration, task.IsWeakTopic));
        }

        return results;
    }

    // Plain (non-record) classes with nullable properties: lets deserialization leave a missing
    // field as null (handled above) instead of System.Text.Json throwing for a record's required
    // positional constructor parameter - same reasoning as NoteAiResponseMapper's raw DTOs.
    private sealed class RawStudyPlanResponse
    {
        [JsonPropertyName("tasks")]
        public List<RawStudyPlanTask>? Tasks { get; set; }
    }

    private sealed class RawStudyPlanTask
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("topic")]
        public string? Topic { get; set; }

        [JsonPropertyName("durationMinutes")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("isWeakTopic")]
        public bool IsWeakTopic { get; set; }
    }
}
