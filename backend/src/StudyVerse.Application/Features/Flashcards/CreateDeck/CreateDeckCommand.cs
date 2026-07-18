using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.CreateDeck;

/// <summary>Creates an empty deck; cards are added afterward one at a time via <c>AddCardCommand</c>.</summary>
public sealed record CreateDeckCommand(Guid UserId, string Title, string? Description) : IRequest<Result<Guid>>;
