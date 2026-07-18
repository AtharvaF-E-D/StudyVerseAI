using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GetFlashcardStats;

/// <summary>Aggregate flashcard stats across all of a user's decks. "Mastered" uses a simple,
/// documented threshold (<c>Repetitions &gt;= 3</c> — three successful reviews in a row without a
/// lapse) rather than any deeper retention model, matching the same "start simple, document the
/// choice" approach used across the rest of the phase.</summary>
public sealed class GetFlashcardStatsQueryHandler : IRequestHandler<GetFlashcardStatsQuery, Result<FlashcardStatsDto>>
{
    /// <summary>See this class's doc comment for why 3 repetitions.</summary>
    public const int MasteredRepetitionsThreshold = 3;

    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetFlashcardStatsQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<FlashcardStatsDto>> Handle(GetFlashcardStatsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var totalDecks = await _db.FlashcardDecks.CountAsync(d => d.UserId == request.UserId, cancellationToken);

        var cardStats = await (
            from card in _db.Flashcards
            join deck in _db.FlashcardDecks on card.DeckId equals deck.Id
            where deck.UserId == request.UserId
            select new { card.NextReviewDateUtc, card.Repetitions }
        ).ToListAsync(cancellationToken);

        var totalCards = cardStats.Count;
        var cardsDueToday = cardStats.Count(c => c.NextReviewDateUtc <= today);
        var masteredCardCount = cardStats.Count(c => c.Repetitions >= MasteredRepetitionsThreshold);

        return Result.Success(new FlashcardStatsDto(totalDecks, totalCards, cardsDueToday, masteredCardCount));
    }
}
