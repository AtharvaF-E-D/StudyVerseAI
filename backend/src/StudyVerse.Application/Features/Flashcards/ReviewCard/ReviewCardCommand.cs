using MediatR;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Flashcards.ReviewCard;

/// <summary>Grades one flashcard review. Ownership is checked via the card's deck
/// (<c>Flashcard.DeckId</c> -&gt; <c>FlashcardDeck.UserId</c>), not a direct UserId column on
/// Flashcard itself.</summary>
public sealed record ReviewCardCommand(Guid UserId, Guid CardId, ReviewQuality Quality)
    : IRequest<Result<ReviewCardResultDto>>;
