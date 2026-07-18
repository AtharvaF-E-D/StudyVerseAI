using MediatR;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GetDeck;

public sealed record GetDeckQuery(Guid UserId, Guid DeckId) : IRequest<Result<DeckDetailDto>>;
