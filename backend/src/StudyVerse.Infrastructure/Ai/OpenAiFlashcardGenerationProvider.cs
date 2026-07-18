using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Infrastructure.Options;

namespace StudyVerse.Infrastructure.Ai;

/// <summary>
/// Calls the real OpenAI Chat Completions API for the Flashcards feature's AI deck generation, via
/// the same official `OpenAI` NuGet package and API key wiring as <see cref="OpenAiChatProvider"/>
/// and <see cref="OpenAiNoteGenerationProvider"/> (see those classes' doc comments for why no
/// separate HttpClient registration is needed). One structured-JSON (JSON mode) call per deck.
/// </summary>
public sealed class OpenAiFlashcardGenerationProvider : IFlashcardGenerationProvider
{
    private const string SystemPromptTemplate =
        "You are StudyVerse AI's flashcard-generation engine. Given a study topic and a desired " +
        "card count, produce a single JSON object (and ONLY a JSON object — no prose, no markdown " +
        "fences) with exactly one key, \"cards\": an array of exactly {0} objects, each shaped " +
        "{{ \"front\", \"back\" }}, where \"front\" is a short question/prompt/term and \"back\" is " +
        "its concise answer/definition. Base the cards on real, accurate knowledge of the topic — " +
        "do not invent facts. Vary the cards so they cover the topic's most important, distinct " +
        "sub-points rather than rephrasing the same fact.";

    private const int MaxOutputTokensPerCard = 120;

    private readonly ChatClient _chatClient;

    public OpenAiFlashcardGenerationProvider(IOptions<OpenAiOptions> openAiOptions, IOptions<AiOptions> aiOptions)
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

    public async Task<IReadOnlyList<(string Front, string Back)>> GenerateFlashcardsAsync(
        string topic, int count, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(string.Format(SystemPromptTemplate, count)),
            ChatMessage.CreateUserMessage($"Topic: {topic}\nCard count: {count}"),
        };

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
            MaxOutputTokenCount = Math.Max(200, count * MaxOutputTokensPerCard),
        };

        var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
        var rawJson = string.Concat(response.Value.Content.Select(part => part.Text));

        var parsed = JsonSerializer.Deserialize<RawFlashcardResponse>(
            rawJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return (parsed?.Cards ?? [])
            .Select(c => (c.Front ?? string.Empty, c.Back ?? string.Empty))
            .Where(c => !string.IsNullOrWhiteSpace(c.Item1) && !string.IsNullOrWhiteSpace(c.Item2))
            .ToList();
    }

    // Plain (non-record) classes with nullable properties, same reasoning as NoteAiResponseMapper's
    // raw DTOs: lets a missing field deserialize to null (handled above) instead of throwing.
    private sealed class RawFlashcardResponse
    {
        [JsonPropertyName("cards")]
        public List<RawFlashcard>? Cards { get; set; }
    }

    private sealed class RawFlashcard
    {
        [JsonPropertyName("front")]
        public string? Front { get; set; }

        [JsonPropertyName("back")]
        public string? Back { get; set; }
    }
}
