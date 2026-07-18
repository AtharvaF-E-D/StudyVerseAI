using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.ToggleDeckFavorite;

public sealed record ToggleDeckFavoriteCommand(Guid UserId, Guid DeckId) : IRequest<Result<bool>>;
