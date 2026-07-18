using System.Text;

namespace StudyVerse.Application.Features.StudyPlanner.Common;

/// <summary>
/// Builds the single user-role prompt sent to <c>IAiChatProvider.GetCompletionAsync</c> (with
/// <c>requireJsonObjectResponse: true</c>) for the Study Planner's AI plan generation. Mirrors
/// <c>MockTestWeaknessPromptBuilder</c>'s pattern of putting the entire task in one user message —
/// that interface has no way to override its fixed tutoring system prompt (see
/// <c>OpenAiChatProvider.SystemPrompt</c>), so the JSON-schema instructions have to live here rather
/// than in a purpose-built system prompt the way <c>OpenAiNoteGenerationProvider</c> gets to.
/// </summary>
internal static class StudyPlanPromptBuilder
{
    /// <summary>
    /// Caps how many days ahead a single plan-generation call is asked to schedule. A real product
    /// would paginate generation for exam dates further out than this (generate the next chunk as
    /// the student progresses); for this time-boxed pass, an exam date beyond this horizon simply
    /// gets its first <see cref="MaxPlanHorizonDays"/> days' worth of tasks generated and persisted
    /// now — see <c>CreateStudyPlanCommandHandler</c>.
    /// </summary>
    public const int MaxPlanHorizonDays = 60;

    /// <summary>The last date this plan generation call should cover: the exam date itself, unless
    /// that's further out than <see cref="MaxPlanHorizonDays"/> allows, in which case it's clamped
    /// to today + (MaxPlanHorizonDays - 1) days.</summary>
    public static DateOnly GetPlanEndDate(DateOnly today, DateOnly examDate)
    {
        var horizonEndDate = today.AddDays(MaxPlanHorizonDays - 1);
        return examDate <= horizonEndDate ? examDate : horizonEndDate;
    }

    public static string Build(
        DateOnly today,
        DateOnly examDate,
        DateOnly planEndDate,
        IReadOnlyList<string> subjects,
        IReadOnlyList<string> weakTopics,
        int hoursPerDayMinutes)
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            $"A student is preparing for an exam on {examDate:yyyy-MM-dd}. Today's date is " +
            $"{today:yyyy-MM-dd}. Build a day-by-day study schedule covering every date from " +
            $"{today:yyyy-MM-dd} through {planEndDate:yyyy-MM-dd} inclusive. Do not schedule " +
            "anything outside that date range, even if the exam is further away than that.");
        sb.AppendLine();

        sb.AppendLine($"Subjects to cover: {string.Join(", ", subjects)}.");
        sb.AppendLine(weakTopics.Count > 0
            ? "Weak topics that need extra focus - give these noticeably MORE study sessions " +
              $"and/or LONGER durations than non-weak topics: {string.Join(", ", weakTopics)}."
            : "No specific weak topics were flagged - distribute sessions evenly across subjects.");
        sb.AppendLine(
            $"Daily study time budget: {hoursPerDayMinutes} minutes per day. The sum of " +
            "durationMinutes for every session on any single date must not exceed this budget.");
        sb.AppendLine();

        sb.AppendLine(
            "Return a single JSON object (and ONLY a JSON object - no prose, no markdown fences) " +
            "with exactly one key, \"tasks\": an array of objects, each shaped { \"date\" (a " +
            "\"YYYY-MM-DD\" string within the range above), \"subject\", \"topic\" (a specific " +
            "sub-topic within that subject, not just a repeat of the subject name), " +
            "\"durationMinutes\" (a positive integer), \"isWeakTopic\" (boolean, true only for " +
            "sessions covering one of the weak topics listed above). Spread sessions across the " +
            "whole date range rather than front-loading, make sure every listed subject is covered " +
            "more than once over the course of the plan, and keep each date's total durationMinutes " +
            "at or under the daily budget.");

        return sb.ToString();
    }
}
