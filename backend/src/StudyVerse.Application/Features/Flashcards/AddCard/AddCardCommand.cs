using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.AddCard;

public sealed record AddCardCommand(Guid UserId, Guid DeckId, string FrontText, string BackText, string? ImageUrl)
    : IRequest<Result<Guid>>;
