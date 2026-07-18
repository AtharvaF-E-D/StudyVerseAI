using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GetDecks;

public sealed class GetDecksQueryHandler : IRequestHandler<GetDecksQuery, Result<IReadOnlyList<DeckSummaryDto>>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetDecksQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<DeckSummaryDto>>> Handle(GetDecksQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var decks = await _db.FlashcardDecks
            .Where(d => d.UserId == request.UserId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .Select(d => new DeckSummaryDto(
                d.Id,
                d.Title,
                d.Description,
                d.IsFavorite,
                d.Cards.Count,
                d.Cards.Count(c => c.NextReviewDateUtc <= today),
                d.ShareToken != null,
                d.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<DeckSummaryDto>>(decks);
    }
}
