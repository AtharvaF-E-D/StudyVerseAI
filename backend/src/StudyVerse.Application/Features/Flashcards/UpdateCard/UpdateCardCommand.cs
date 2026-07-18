using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.UpdateCard;

public sealed record UpdateCardCommand(Guid UserId, Guid DeckId, Guid CardId, string FrontText, string BackText, string? ImageUrl)
    : IRequest<Result>;
