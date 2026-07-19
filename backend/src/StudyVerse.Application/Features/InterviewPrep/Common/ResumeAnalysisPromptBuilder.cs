namespace StudyVerse.Application.Features.InterviewPrep.Common;

/// <summary>
/// Builds the single user-role prompt sent to <c>IAiChatProvider.GetCompletionAsync(...,
/// requireJsonObjectResponse: true)</c> for resume analysis — same reasoning/shape as
/// <c>NewsQuizPromptBuilder</c>: no way to override the fixed tutoring system prompt, so the task
/// instructions live entirely in this one user-role message.
/// </summary>
internal static class ResumeAnalysisPromptBuilder
{
    /// <summary>Guards against blowing the model's context window on an unusually long resume —
    /// same reasoning as <c>OpenAiNoteGenerationProvider.MaxSourceTextLength</c>.</summary>
    private const int MaxResumeTextLength = 8_000;

    public static string Build(string resumeText)
    {
        var truncatedText = resumeText.Length > MaxResumeTextLength ? resumeText[..MaxResumeTextLength] : resumeText;

        return
            "A candidate just uploaded their resume for review ahead of job interviews. Below is the " +
            "extracted text of that resume.\n\n" +
            $"Resume text:\n{truncatedText}\n\n" +
            "Analyze THIS resume's actual content for overall quality, clarity, and impact — reference " +
            "the candidate's real experience, wording, and structure rather than giving generic resume " +
            "advice. Respond with ONLY a JSON object (no prose, no markdown fences) with exactly this shape:\n" +
            "{ \"overallScore\": 0-100 integer, \"strengths\": [3 to 5 strings specific to this resume], " +
            "\"weaknesses\": [3 to 5 strings specific to this resume], \"suggestions\": [3 to 5 " +
            "actionable strings specific to this resume] }";
    }
}
