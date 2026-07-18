using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// One front/back study card belonging to a <see cref="FlashcardDeck"/>, plus its standard SM-2
/// spaced-repetition scheduling state (see <see cref="Domain.SpacedRepetition.Sm2Scheduler"/> for
/// the algorithm that advances these fields on each review). A brand-new card starts at
/// <see cref="Domain.SpacedRepetition.Sm2Scheduler.InitialEaseFactor"/>/0/0 with
/// <see cref="NextReviewDateUtc"/> set to the day it was created, so it's immediately due — no
/// separate "new card" queue distinct from the due-cards queue.
/// </summary>
public class Flashcard
{
    public Guid Id { get; set; }

    public Guid DeckId { get; set; }

    public string FrontText { get; set; } = string.Empty;

    public string BackText { get; set; } = string.Empty;

    /// <summary>Just a URL field for now — no upload pipeline. A future pass could let this point
    /// at an <c>IFileStorageService</c>-managed file the way <see cref="Note"/> uploads do.</summary>
    public string? ImageUrl { get; set; }

    public double EaseFactor { get; set; } = Sm2Scheduler.InitialEaseFactor;

    public int IntervalDays { get; set; }

    public int Repetitions { get; set; }

    public DateOnly NextReviewDateUtc { get; set; }

    public DateTime? LastReviewedAtUtc { get; set; }

    public FlashcardDeck? Deck { get; set; }
}
