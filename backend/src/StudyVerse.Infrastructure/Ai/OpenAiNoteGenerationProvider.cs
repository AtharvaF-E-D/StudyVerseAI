using Microsoft.Extensions.Options;
using OpenAI.Chat;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Infrastructure.Options;

namespace StudyVerse.Infrastructure.Ai;

/// <summary>
/// Calls the real OpenAI Chat Completions API for the AI Notes feature, via the same official
/// `OpenAI` NuGet package and API key wiring as <see cref="OpenAiChatProvider"/> (see that class's
/// doc comment for why no separate HttpClient registration is needed). Kept as a separate class
/// because its two calls — image transcription and structured-JSON note generation — have their
/// own personas and request shapes distinct from the tutoring chat.
/// </summary>
public sealed class OpenAiNoteGenerationProvider : INoteGenerationProvider
{
    private const string ImageDescriptionSystemPrompt =
        "You transcribe and describe academic content from images for a study notes app. Read " +
        "any text, diagrams, equations, charts, and labels visible in the image and produce a " +
        "thorough plain-text transcription a student could study from on its own, preserving " +
        "structure (headings, bullet points, equations) using plain text formatting.";

    private const string NoteGenerationSystemPrompt =
        "You are StudyVerse AI's note-generation engine. Given the extracted text of a student's " +
        "uploaded document or transcribed image, produce a single JSON object (and ONLY a JSON " +
        "object — no prose, no markdown fences) with exactly these keys:\n" +
        "- \"summary\": a concise paragraph summarizing the material.\n" +
        "- \"keyPoints\": an array of short strings, the most important takeaways (aim for 5-10).\n" +
        "- \"flashcards\": an array of objects { \"question\", \"answer\" } for active recall (aim for 5-10).\n" +
        "- \"mcqs\": an array of objects { \"question\", \"options\" (exactly 4 strings), " +
        "\"correctOptionIndex\" (0-based index into options), \"explanation\" } (aim for 5).\n" +
        "- \"mindMap\": a single nested object { \"topic\", \"children\": [...] } representing the " +
        "material as an outline tree — the root topic with sub-topics recursively as children; " +
        "leaf nodes have an empty children array. This is an indented outline, not a visual diagram.\n" +
        "- \"revisionSheet\": a compact markdown-formatted quick-revision sheet (headings + bullet points).\n" +
        "- \"vocabulary\": an array of objects { \"term\", \"definition\" } for key terminology (aim for 5-10).\n" +
        "- \"formulas\": an array of objects { \"name\", \"formula\", \"explanation\" } for any " +
        "formulas/equations present in the material (an empty array if the subject has none).\n" +
        "Base everything strictly on the provided source text — do not invent facts it doesn't support.";

    /// <summary>Guards against blowing the model's context window on very large documents. A future
    /// pass could chunk-and-summarize hierarchically instead; out of scope for this pass.</summary>
    private const int MaxSourceTextLength = 24_000;

    private const int MaxNoteGenerationOutputTokens = 4_000;

    private readonly ChatClient _chatClient;

    public OpenAiNoteGenerationProvider(IOptions<OpenAiOptions> openAiOptions, IOptions<AiOptions> aiOptions)
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

    public async Task<string> DescribeImageAsync(Stream imageContent, string mediaType, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await imageContent.CopyToAsync(buffer, cancellationToken);
        var imageBytes = BinaryData.FromBytes(buffer.ToArray());

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(ImageDescriptionSystemPrompt),
            ChatMessage.CreateUserMessage(
            [
                ChatMessageContentPart.CreateTextPart("Transcribe and describe the academic content in this image."),
                ChatMessageContentPart.CreateImagePart(imageBytes, mediaType),
            ]),
        };

        var response = await _chatClient.CompleteChatAsync(messages, options: null, cancellationToken);
        return string.Concat(response.Value.Content.Select(part => part.Text));
    }

    public async Task<string> GenerateNoteContentJsonAsync(string sourceText, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(NoteGenerationSystemPrompt),
            ChatMessage.CreateUserMessage(Truncate(sourceText, MaxSourceTextLength)),
        };

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
            MaxOutputTokenCount = MaxNoteGenerationOutputTokens,
        };

        var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
        return string.Concat(response.Value.Content.Select(part => part.Text));
    }

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength];
}
