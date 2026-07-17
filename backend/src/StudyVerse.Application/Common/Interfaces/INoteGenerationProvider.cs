namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Sibling to <see cref="IAiChatProvider"/> for the two OpenAI calls the AI Notes feature makes:
/// describing/transcribing an uploaded image (used as that image's "extracted text" in lieu of a
/// separate OCR pipeline — see <c>DocumentTextExtractionService</c>), and the single
/// structured-JSON call that turns a note's extracted text into all seven pieces of generated
/// content (summary, key points, flashcards, mcqs, mind map, revision sheet, vocabulary, formulas)
/// in one round trip. Kept as its own interface rather than added to <see cref="IAiChatProvider"/>
/// because these calls have a different shape (JSON mode, image input parts) and a different
/// persona/system prompt than the tutoring chat — bundling them would make IAiChatProvider's
/// contract harder to reason about for both existing tutor callers and these new ones.
/// </summary>
public interface INoteGenerationProvider
{
    /// <summary>
    /// Sends the image directly to a vision-capable chat completion asking it to transcribe/
    /// describe the academic content (text, diagrams, equations, labels) as plain text.
    /// </summary>
    /// <param name="imageContent">The raw image bytes.</param>
    /// <param name="mediaType">The image's MIME type, e.g. <c>"image/png"</c> or <c>"image/jpeg"</c>.</param>
    Task<string> DescribeImageAsync(Stream imageContent, string mediaType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests all of a note's generated content back as a single JSON object (OpenAI JSON mode),
    /// given the note's extracted text. The returned string is the raw JSON response body — parsing
    /// and validation is the caller's job (see <c>NoteAiResponseMapper</c>), since a malformed or
    /// unexpectedly-shaped response is an expected failure mode this interface doesn't hide.
    /// </summary>
    Task<string> GenerateNoteContentJsonAsync(string sourceText, CancellationToken cancellationToken = default);
}
