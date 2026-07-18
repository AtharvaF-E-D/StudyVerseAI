using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GetDueCards;

public sealed class GetDueCardsQueryHandler : IRequestHandler<GetDueCardsQuery, Result<IReadOnlyList<DueCardDto>>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetDueCardsQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<DueCardDto>>> Handle(GetDueCardsQuery request, CancellationToken cancellationToken)
    {
        if (request.DeckId is { } deckId)
        {
            var deckExists = await _db.FlashcardDecks.AnyAsync(
                d => d.Id == deckId && d.UserId == request.UserId, cancellationToken);

            if (!deckExists)
            {
                return Result.Failure<IReadOnlyList<DueCardDto>>("Deck not found.", ResultErrorType.NotFound);
            }
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var dueCards = await (
            from card in _db.Flashcards
            join deck in _db.FlashcardDecks on card.DeckId equals deck.Id
            where deck.UserId == request.UserId
                && card.NextReviewDateUtc <= today
                && (request.DeckId == null || deck.Id == request.DeckId)
            orderby card.NextReviewDateUtc
            select new DueCardDto(card.Id, deck.Id, deck.Title, card.FrontText, card.BackText, card.ImageUrl, card.NextReviewDateUtc)
        ).ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<DueCardDto>>(dueCards);
    }
}
