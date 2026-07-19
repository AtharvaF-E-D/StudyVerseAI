namespace StudyVerse.Application.Features.CodingPractice.Common;

/// <summary>
/// Builds the single user-role prompt sent to <c>IAiChatProvider.GetCompletionAsync</c> for a
/// coding-hint request. Reuses the AI tutor's existing chat provider rather than a new AI
/// abstraction, the same "instructions live entirely in one user-role message" approach
/// <c>NewsQuizPromptBuilder</c>/<c>MockTestWeaknessPromptBuilder</c> take, since
/// <see cref="StudyVerse.Application.Common.Interfaces.IAiChatProvider"/> owns a fixed system
/// prompt callers can't override.
///
/// The anti-answer-leak instruction below is the whole point of this prompt: a hint that just
/// hands over working code defeats the purpose of "practice", so the prompt explicitly forbids
/// full/runnable code and forbids naming the exact algorithm outright, asking instead for one short
/// conceptual nudge (e.g. "think about what data structure gives O(1) lookups here" rather than
/// "use a hash map keyed by ...").
/// </summary>
internal static class CodingHintPromptBuilder
{
    /// <summary>Guards against blowing the model's context window on an unusually long in-progress
    /// solution - same reasoning as <c>OpenAiNoteGenerationProvider.MaxSourceTextLength</c>.</summary>
    private const int MaxCodeLength = 4_000;

    public static string Build(string problemTitle, string problemDescription, string currentCode)
    {
        var truncatedCode = currentCode.Length > MaxCodeLength ? currentCode[..MaxCodeLength] : currentCode;
        var codeSection = string.IsNullOrWhiteSpace(truncatedCode)
            ? "(The student hasn't written any code yet.)"
            : truncatedCode;

        return
            "A student is working on this coding practice problem and is stuck. Give them exactly ONE " +
            "short, concise hint (2-3 sentences at most) that nudges them toward the right approach.\n\n" +
            $"Problem: {problemTitle}\n{problemDescription}\n\n" +
            $"The student's current (possibly broken or incomplete) code:\n{codeSection}\n\n" +
            "STRICT RULES for your hint:\n" +
            "1. Do NOT write any working code, function bodies, or a full solution - no code blocks " +
            "at all.\n" +
            "2. Do NOT name the exact algorithm or data structure outright (e.g. don't just say " +
            "\"use a hash map\" or \"use dynamic programming\") - instead nudge toward the underlying " +
            "idea (e.g. \"what if you could look up whether a value exists in constant time?\").\n" +
            "3. Point out what's wrong with their current approach (if anything) or what edge case " +
            "they may be missing, without solving it for them.\n" +
            "4. Respond with plain prose only - just the hint text, no preamble like \"Hint:\", no " +
            "markdown, no code.";
    }
}
