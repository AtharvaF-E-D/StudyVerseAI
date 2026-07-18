using System.Text;

namespace StudyVerse.Application.Features.MockTests.Common;

/// <summary>One wrong answer, with enough context (category, the question, what the student picked
/// vs. the real answer) for the AI weakness-analysis prompt to reference specifics rather than
/// generic filler.</summary>
internal sealed record MockTestWrongAnswer(
    string Category,
    string QuestionText,
    string SelectedAnswerText,
    string CorrectAnswerText);

/// <summary>
/// Builds the single user-role prompt sent to <c>IAiChatProvider.GetCompletionAsync</c> for a mock
/// test's AI weakness analysis. That interface has no way to override its fixed tutoring system
/// prompt (see <c>OpenAiChatProvider.SystemPrompt</c>) — reusing it rather than standing up a new
/// AI provider abstraction (per the phase spec) means the task instructions have to live entirely
/// in this one user-role message instead of a purpose-built system prompt.
/// </summary>
internal static class MockTestWeaknessPromptBuilder
{
    public static string Build(IReadOnlyList<MockTestWrongAnswer> wrongAnswers)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "A student just submitted a mock test. Below are the questions they got wrong, grouped " +
            "by category, with their answer and the correct answer.");
        sb.AppendLine();

        foreach (var group in wrongAnswers.GroupBy(w => w.Category))
        {
            sb.AppendLine($"Category: {group.Key}");
            foreach (var wrong in group)
            {
                sb.AppendLine(
                    $"- Question: \"{wrong.QuestionText}\" | Student's answer: \"{wrong.SelectedAnswerText}\" | " +
                    $"Correct answer: \"{wrong.CorrectAnswerText}\"");
            }

            sb.AppendLine();
        }

        sb.AppendLine(
            "Write a short (3-4 sentence) weakness analysis identifying which topic areas need work " +
            "and one concrete study suggestion. Address the student directly (\"you\"), and do not " +
            "repeat or quote these instructions back.");

        return sb.ToString();
    }
}
