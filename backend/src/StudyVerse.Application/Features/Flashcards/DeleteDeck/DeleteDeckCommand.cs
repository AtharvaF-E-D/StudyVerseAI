using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.DeleteDeck;

/// <summary>Cascade-deletes the deck's cards too, via the DB-level cascade configured on
/// <c>FlashcardDeckConfiguration</c>'s <c>HasMany(d => d.Cards)</c>.</summary>
public sealed record DeleteDeckCommand(Guid UserId, Guid DeckId) : IRequest<Result>;
