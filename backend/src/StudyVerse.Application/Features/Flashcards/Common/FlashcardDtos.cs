namespace StudyVerse.Application.Features.Flashcards.Common;

/// <summary>One deck as it appears in "my decks" (<c>GetDecksQuery</c>) — summary counts only, no card list.</summary>
public sealed record DeckSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsFavorite,
    int CardCount,
    int DueTodayCount,
    bool IsShared,
    DateTime CreatedAtUtc);

/// <summary>One flashcard as returned in a deck's full card list.</summary>
public sealed record FlashcardCardDto(
    Guid Id,
    string FrontText,
    string BackText,
    string? ImageUrl,
    double EaseFactor,
    int IntervalDays,
    int Repetitions,
    DateOnly NextReviewDateUtc,
    DateTime? LastReviewedAtUtc);

/// <summary>A full deck with all of its cards — <c>GetDeckQuery</c> (owner view).</summary>
public sealed record DeckDetailDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsFavorite,
    bool IsShared,
    Guid? SourceNoteId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<FlashcardCardDto> Cards);

/// <summary>The read-only public view of a shared deck (<c>GetSharedDeckQuery</c>) — deliberately
/// omits <see cref="DeckDetailDto.IsFavorite"/> and any owner-only scheduling fields a stranger
/// following a share link has no business seeing or acting on; still shows each card's front/back
/// so it's actually useful to study from.</summary>
public sealed record SharedDeckDto(
    string Title,
    string? Description,
    DateTime CreatedAtUtc,
    IReadOnlyList<SharedFlashcardDto> Cards);

public sealed record SharedFlashcardDto(string FrontText, string BackText, string? ImageUrl);

/// <summary>Result of sharing/unsharing — the token itself, so the caller can build a share link.</summary>
public sealed record ShareDeckResultDto(Guid DeckId, string ShareToken);

/// <summary>One due card in the cross-deck daily review queue (<c>GetDueCardsQuery</c>) — includes
/// which deck it belongs to since the queue can span every deck the user owns.</summary>
public sealed record DueCardDto(
    Guid Id,
    Guid DeckId,
    string DeckTitle,
    string FrontText,
    string BackText,
    string? ImageUrl,
    DateOnly NextReviewDateUtc);

/// <summary>Result of grading one review — the card's new SM-2 state, so the client can show the
/// interval/ease-factor progression immediately without a separate re-fetch.</summary>
public sealed record ReviewCardResultDto(
    Guid CardId,
    double EaseFactor,
    int IntervalDays,
    int Repetitions,
    DateOnly NextReviewDateUtc,
    DateTime LastReviewedAtUtc);

/// <summary>Aggregate flashcard stats across all of a user's decks (<c>GetFlashcardStatsQuery</c>).
/// "Mastered" is a simple, documented threshold — <see cref="MasteredCardCount"/> counts cards with
/// <c>Repetitions &gt;= 3</c> (three successful reviews in a row without a lapse), not a claim about
/// long-term retention.</summary>
public sealed record FlashcardStatsDto(
    int TotalDecks,
    int TotalCards,
    int CardsDueToday,
    int MasteredCardCount);
