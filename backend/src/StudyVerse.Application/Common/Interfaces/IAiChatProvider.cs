using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Abstraction over whatever LLM powers the AI tutor, so the Application layer never references
/// the OpenAI SDK directly (mirrors the <c>IEmailSender</c>/<c>IOtpSender</c> split: interface
/// here, provider-specific implementation in Infrastructure). The tutoring system prompt
/// (persona + the LaTeX/code formatting contract the mobile client renders against) is owned by
/// the implementation, not passed in here — callers only ever supply the conversation history.
/// </summary>
public interface IAiChatProvider
{
    /// <summary>
    /// <paramref name="requireJsonObjectResponse"/> requests OpenAI's structured JSON response mode
    /// (<c>ChatResponseFormat.CreateJsonObjectFormat()</c> — the same mechanism
    /// <c>OpenAiNoteGenerationProvider</c>/<c>OpenAiFlashcardGenerationProvider</c> use) instead of
    /// free-form prose, for callers that need a machine-parseable reply (e.g. the Study Planner's AI
    /// plan generation) rather than a tutoring answer. This is the one knob added on top of the
    /// fixed tutoring system prompt — callers still can't override that persona, only ask for its
    /// output to be shaped as JSON. Deliberately placed after <paramref name="cancellationToken"/>,
    /// not before it (the more usual spot for a non-cancellation parameter), purely so the existing
    /// call sites that already pass <c>cancellationToken</c> positionally keep compiling unchanged.
    /// </summary>
    Task<AiChatResult> GetCompletionAsync(
        IReadOnlyList<AiChatMessage> history,
        CancellationToken cancellationToken = default,
        bool requireJsonObjectResponse = false);

    /// <summary>
    /// Makes one additional lightweight completion call to suggest 2-3 short, natural follow-up
    /// questions given the conversation so far. <paramref name="history"/> should include the
    /// assistant reply that was just generated. Best-effort: implementations should let this fail
    /// independently of <see cref="GetCompletionAsync"/> — callers treat an empty list the same as
    /// "no suggestions available", never as a reason to fail the whole request.
    /// </summary>
    Task<IReadOnlyList<string>> GetSuggestedFollowUpsAsync(IReadOnlyList<AiChatMessage> history, CancellationToken cancellationToken = default);
}

public sealed record AiChatMessage(MessageRole Role, string Content);

public sealed record AiChatResult(string Content, int PromptTokens, int CompletionTokens);
