namespace StudyVerse.Domain.Entities;

/// <summary>
/// A named collection of <see cref="Flashcard"/>s belonging to one user. A deck is either built
/// manually (empty at creation, cards added one at a time), generated wholesale from an AI topic
/// prompt, or copied from an existing <see cref="Note"/>'s already-AI-generated flashcards — see
/// <see cref="SourceNoteId"/>, set only for that third path (purely informational provenance; a
/// deck's cards are its own independent rows from that point on, not a live view of the note).
///
/// <see cref="ShareToken"/> is null until the owner explicitly shares the deck (<c>ShareDeckCommand</c>),
/// at which point it's a short random public token that lets anyone with the link view the deck
/// read-only via <c>GetSharedDeckQuery</c> — no authentication, no ownership check, just "does this
/// token exist and is it still set". <c>UnshareDeckCommand</c> clears it back to null, immediately
/// invalidating the old link.
/// </summary>
public class FlashcardDeck
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsFavorite { get; set; }

    /// <summary>Set only when this deck was generated from an existing note's flashcards
    /// (<c>GenerateDeckFromNoteCommand</c>); null for manually-created and topic-generated decks.</summary>
    public Guid? SourceNoteId { get; set; }

    /// <summary>Null unless the deck is currently shared — see this class's doc comment.</summary>
    public string? ShareToken { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public User? User { get; set; }

    public Note? SourceNote { get; set; }

    public ICollection<Flashcard> Cards { get; set; } = new List<Flashcard>();
}
