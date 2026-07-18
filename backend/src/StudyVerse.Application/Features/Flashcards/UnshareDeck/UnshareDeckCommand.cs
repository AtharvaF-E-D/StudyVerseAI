using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.UnshareDeck;

/// <summary>Clears <c>ShareToken</c>, immediately invalidating any previously-issued share link.</summary>
public sealed record UnshareDeckCommand(Guid UserId, Guid DeckId) : IRequest<Result>;
