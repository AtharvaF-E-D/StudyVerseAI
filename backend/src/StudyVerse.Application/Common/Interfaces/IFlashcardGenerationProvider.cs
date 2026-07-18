namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Sibling to <see cref="INoteGenerationProvider"/> for the Flashcards feature's own AI call:
/// given a topic and a desired card count, produce that many real front/back flashcard pairs in
/// one structured-JSON round trip. Kept as its own interface (rather than reusing
/// <see cref="INoteGenerationProvider"/>) because it has a different persona/prompt and doesn't
/// need that interface's image-transcription or seven-piece note-content shape — just a flat list
/// of pairs.
/// </summary>
public interface IFlashcardGenerationProvider
{
    /// <summary>
    /// Requests <paramref name="count"/> front/back flashcard pairs on <paramref name="topic"/>
    /// back as parsed data (unlike <see cref="INoteGenerationProvider.GenerateNoteContentJsonAsync"/>,
    /// which hands the caller raw JSON to parse itself — this call's response shape is simple
    /// enough that the provider can safely own the JSON parsing).
    /// </summary>
    Task<IReadOnlyList<(string Front, string Back)>> GenerateFlashcardsAsync(
        string topic, int count, CancellationToken cancellationToken = default);
}
