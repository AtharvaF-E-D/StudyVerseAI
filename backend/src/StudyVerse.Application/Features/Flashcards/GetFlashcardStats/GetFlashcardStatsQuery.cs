using MediatR;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GetFlashcardStats;

public sealed record GetFlashcardStatsQuery(Guid UserId) : IRequest<Result<FlashcardStatsDto>>;
