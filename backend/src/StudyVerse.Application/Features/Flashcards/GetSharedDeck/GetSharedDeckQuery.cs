using MediatR;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GetSharedDeck;

/// <summary>Deliberately carries no <c>UserId</c> — this is the one Flashcards query with no
/// authentication or ownership check at all, resolved purely by the public <see cref="ShareToken"/>.
/// See <see cref="Domain.Entities.FlashcardDeck.ShareToken"/>'s doc comment.</summary>
public sealed record GetSharedDeckQuery(string ShareToken) : IRequest<Result<SharedDeckDto>>;
