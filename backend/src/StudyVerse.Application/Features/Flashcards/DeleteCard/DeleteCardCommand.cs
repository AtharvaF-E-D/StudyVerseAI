using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.DeleteCard;

public sealed record DeleteCardCommand(Guid UserId, Guid DeckId, Guid CardId) : IRequest<Result>;
