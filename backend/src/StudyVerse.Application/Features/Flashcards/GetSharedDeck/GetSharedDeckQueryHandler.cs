using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GetSharedDeck;

public sealed class GetSharedDeckQueryHandler : IRequestHandler<GetSharedDeckQuery, Result<SharedDeckDto>>
{
    private readonly IAppDbContext _db;

    public GetSharedDeckQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<SharedDeckDto>> Handle(GetSharedDeckQuery request, CancellationToken cancellationToken)
    {
        // ShareToken is unique and only ever non-null while the deck is actively shared (see
        // FlashcardDeckConfiguration) - a null/mismatched/no-longer-shared token all fail the same
        // "not found" way, so a stranger probing tokens learns nothing about which ones ever existed.
        var deck = await _db.FlashcardDecks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.ShareToken == request.ShareToken, cancellationToken);

        if (deck is null)
        {
            return Result.Failure<SharedDeckDto>("This shared deck link is invalid or no longer active.", ResultErrorType.NotFound);
        }

        // No stable "creation order" field exists on Flashcard (see that entity's doc comment) -
        // returned in whatever order EF/the DB yields them, which is fine for a read-only public view.
        var cards = deck.Cards
            .Select(c => new SharedFlashcardDto(c.FrontText, c.BackText, c.ImageUrl))
            .ToList();

        return Result.Success(new SharedDeckDto(deck.Title, deck.Description, deck.CreatedAtUtc, cards));
    }
}
