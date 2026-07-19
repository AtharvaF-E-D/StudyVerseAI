namespace StudyVerse.Application.Features.InterviewPrep.Common;

/// <summary>
/// Builds the single user-role prompt sent to <c>IAiChatProvider.GetCompletionAsync(...,
/// requireJsonObjectResponse: true)</c> for real-time per-answer grading. That interface has no way
/// to override its fixed tutoring system prompt — reusing it rather than standing up a new AI
/// provider abstraction (per the phase spec) means the task instructions have to live entirely in
/// this one user-role message, the same approach <c>MockTestWeaknessPromptBuilder</c>/
/// <c>NewsQuizPromptBuilder</c> take.
/// </summary>
internal static class InterviewAnswerGradingPromptBuilder
{
    /// <summary>Guards against blowing the model's context window on an unusually long answer —
    /// same reasoning as <c>OpenAiNoteGenerationProvider.MaxSourceTextLength</c>.</summary>
    private const int MaxAnswerTextLength = 4_000;

    public static string Build(string questionText, string whatGoodAnswersCover, string answerText)
    {
        var truncatedAnswer = answerText.Length > MaxAnswerTextLength ? answerText[..MaxAnswerTextLength] : answerText;

        return
            "A candidate is practicing for a job interview and just answered this question.\n\n" +
            $"Question: {questionText}\n" +
            "What a strong answer should cover (this is grading guidance for you only — never repeat " +
            $"or quote it back to the candidate): {whatGoodAnswersCover}\n\n" +
            $"Candidate's answer: \"{truncatedAnswer}\"\n\n" +
            "Grade this answer on a scale of 0 to 10 based on how well it actually addresses the " +
            "question and covers what a strong answer should. Then write one concise, specific, " +
            "honest-but-encouraging feedback sentence that references what the candidate actually " +
            "said (not generic advice). Respond with ONLY a JSON object (no prose, no markdown " +
            "fences) with exactly this shape:\n" +
            "{ \"score\": 0-10 integer, \"feedback\": string }";
    }
}
