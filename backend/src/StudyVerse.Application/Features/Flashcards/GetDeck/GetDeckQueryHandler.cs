using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GetDeck;

public sealed class GetDeckQueryHandler : IRequestHandler<GetDeckQuery, Result<DeckDetailDto>>
{
    private readonly IAppDbContext _db;

    public GetDeckQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<DeckDetailDto>> Handle(GetDeckQuery request, CancellationToken cancellationToken)
    {
        var deck = await _db.FlashcardDecks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == request.DeckId && d.UserId == request.UserId, cancellationToken);

        if (deck is null)
        {
            return Result.Failure<DeckDetailDto>("Deck not found.", ResultErrorType.NotFound);
        }

        var cards = deck.Cards
            .OrderBy(c => c.NextReviewDateUtc)
            .Select(c => new FlashcardCardDto(
                c.Id, c.FrontText, c.BackText, c.ImageUrl, c.EaseFactor, c.IntervalDays, c.Repetitions,
                c.NextReviewDateUtc, c.LastReviewedAtUtc))
            .ToList();

        var dto = new DeckDetailDto(
            deck.Id,
            deck.Title,
            deck.Description,
            deck.IsFavorite,
            deck.ShareToken != null,
            deck.SourceNoteId,
            deck.CreatedAtUtc,
            deck.UpdatedAtUtc,
            cards);

        return Result.Success(dto);
    }
}
