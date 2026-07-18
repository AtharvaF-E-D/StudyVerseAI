using MediatR;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.ShareDeck;

/// <summary>Generates (or, if already shared, returns the existing) <c>ShareToken</c> for a deck.</summary>
public sealed record ShareDeckCommand(Guid UserId, Guid DeckId) : IRequest<Result<ShareDeckResultDto>>;
