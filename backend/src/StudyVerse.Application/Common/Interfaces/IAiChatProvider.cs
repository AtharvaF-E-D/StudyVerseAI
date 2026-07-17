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
    Task<AiChatResult> GetCompletionAsync(IReadOnlyList<AiChatMessage> history, CancellationToken cancellationToken = default);

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
