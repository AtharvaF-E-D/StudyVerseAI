using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// A single turn (user question or assistant reply) within a <see cref="Conversation"/>.
/// <see cref="Content"/> is mapped to a <c>text</c> database column (see
/// <c>MessageConfiguration</c>) since assistant replies can be long. <see cref="PromptTokens"/>/
/// <see cref="CompletionTokens"/> are the token counts the AI provider reported for the
/// completion that produced this message; they are only ever populated on Assistant messages and
/// stay null on User messages.
/// </summary>
public class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public MessageRole Role { get; set; }

    public string Content { get; set; } = string.Empty;

    public int? PromptTokens { get; set; }

    public int? CompletionTokens { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Conversation? Conversation { get; set; }
}
