using System.Text;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.Common;

/// <summary>One graded Q&amp;A pair from a just-finished session, in presentation order — enough
/// context for the improvement-plan prompt to reference specifics rather than generic filler.</summary>
public sealed record GradedQaPair(string QuestionText, string AnswerText, int Score, string Feedback);

/// <summary>
/// Builds the single user-role prompt sent to <c>IAiChatProvider.GetCompletionAsync</c> for a just-
/// completed session's improvement plan — the same reasoning/shape as
/// <c>MockTestWeaknessPromptBuilder</c>: no way to override the fixed tutoring system prompt, so the
/// task instructions live entirely in this one user-role message.
/// </summary>
internal static class InterviewImprovementPlanPromptBuilder
{
    public static string Build(InterviewQuestionType type, IReadOnlyList<GradedQaPair> pairs)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            $"A candidate just completed a {type} mock interview practice session with " +
            $"{pairs.Count} questions, each already graded 0-10. Below are the questions, their " +
            "answers, and the scores/feedback given.");
        sb.AppendLine();

        for (var i = 0; i < pairs.Count; i++)
        {
            var pair = pairs[i];
            sb.AppendLine($"Q{i + 1}: \"{pair.QuestionText}\"");
            sb.AppendLine($"Candidate's answer: \"{pair.AnswerText}\"");
            sb.AppendLine($"Score given: {pair.Score}/10 | Feedback given: \"{pair.Feedback}\"");
            sb.AppendLine();
        }

        sb.AppendLine(
            "Write a real, specific improvement plan for this candidate in 2-3 concrete paragraphs, " +
            "referencing actual patterns across their answers above (not generic interview advice). " +
            "Address the candidate directly (\"you\"), and do not repeat or quote these instructions back.");

        return sb.ToString();
    }
}
