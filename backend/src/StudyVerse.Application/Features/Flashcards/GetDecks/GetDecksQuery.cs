using MediatR;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GetDecks;

public sealed record GetDecksQuery(Guid UserId) : IRequest<Result<IReadOnlyList<DeckSummaryDto>>>;
