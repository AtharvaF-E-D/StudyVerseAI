using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Enums;
using StudyVerse.Infrastructure.Options;

namespace StudyVerse.Infrastructure.Ai;

/// <summary>
/// Calls the real OpenAI Chat Completions API via the official `OpenAI` NuGet package (the
/// actively-maintained SDK published by OpenAI itself, not a community wrapper). The underlying
/// <see cref="ChatClient"/> manages its own HTTP transport (built on System.ClientModel), so —
/// unlike <c>AppleTokenValidator</c>, which calls a plain REST endpoint directly — no separate
/// <c>HttpClient</c> registration is needed in DI for this provider.
/// </summary>
public sealed partial class OpenAiChatProvider : IAiChatProvider
{
    /// <summary>
    /// The tutoring persona and formatting contract. Fixed here (not caller-supplied) so the
    /// mobile client always gets a stable shape to render against: LaTeX via $inline$/$$block$$
    /// delimiters and fenced, language-tagged code blocks.
    /// </summary>
    private const string SystemPrompt =
        "You are StudyVerse AI's tutor. Help students understand concepts clearly and encourage " +
        "active learning. Use $inline$ and $$block$$ LaTeX math delimiters for any mathematical " +
        "notation, and fenced code blocks with a language tag for any code.";

    private const string FollowUpSystemPrompt =
        "Based on the conversation so far, suggest 2 to 3 short, natural follow-up questions a " +
        "student might want to ask next. Reply with ONLY the questions, one per line — no " +
        "numbering, no bullet points, no extra commentary.";

    private const int MaxFollowUps = 3;

    private readonly ChatClient _chatClient;

    public OpenAiChatProvider(IOptions<OpenAiOptions> openAiOptions, IOptions<AiOptions> aiOptions)
    {
        var apiKey = openAiOptions.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI:ApiKey is not configured. Set it via `dotnet user-secrets set OpenAI:ApiKey <key>` " +
                "in Development, or the OpenAI__ApiKey environment variable in Staging/Production.");
        }

        _chatClient = new ChatClient(aiOptions.Value.Model, apiKey);
    }

    public async Task<AiChatResult> GetCompletionAsync(IReadOnlyList<AiChatMessage> history, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage> { ChatMessage.CreateSystemMessage(SystemPrompt) };
        messages.AddRange(history.Select(ToChatMessage));

        var response = await _chatClient.CompleteChatAsync(messages, options: null, cancellationToken);
        var completion = response.Value;

        var content = string.Concat(completion.Content.Select(part => part.Text));

        return new AiChatResult(
            content,
            completion.Usage?.InputTokenCount ?? 0,
            completion.Usage?.OutputTokenCount ?? 0);
    }

    public async Task<IReadOnlyList<string>> GetSuggestedFollowUpsAsync(IReadOnlyList<AiChatMessage> history, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage> { ChatMessage.CreateSystemMessage(FollowUpSystemPrompt) };
        messages.AddRange(history.Select(ToChatMessage));

        var options = new ChatCompletionOptions { MaxOutputTokenCount = 200 };
        var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
        var completion = response.Value;

        var raw = string.Concat(completion.Content.Select(part => part.Text));

        return raw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => LeadingListMarkerRegex().Replace(line, string.Empty).Trim())
            .Where(line => line.Length > 0)
            .Take(MaxFollowUps)
            .ToList();
    }

    private static ChatMessage ToChatMessage(AiChatMessage message) => message.Role switch
    {
        MessageRole.User => ChatMessage.CreateUserMessage(message.Content),
        MessageRole.Assistant => ChatMessage.CreateAssistantMessage(message.Content),
        _ => throw new ArgumentOutOfRangeException(nameof(message), message.Role, "Unsupported chat message role."),
    };

    // Strips a leading "1.", "1)", "-", "*", or "•" list marker some models add despite being
    // asked not to, so parsing is robust even when the model doesn't follow the format exactly.
    [GeneratedRegex(@"^\s*(?:\d+[\.\)]|[-*•])\s*")]
    private static partial Regex LeadingListMarkerRegex();
}
