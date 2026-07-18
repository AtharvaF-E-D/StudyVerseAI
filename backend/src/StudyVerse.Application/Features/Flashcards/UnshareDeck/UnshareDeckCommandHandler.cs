using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.UnshareDeck;

public sealed class UnshareDeckCommandHandler : IRequestHandler<UnshareDeckCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UnshareDeckCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(UnshareDeckCommand request, CancellationToken cancellationToken)
    {
        var deck = await _db.FlashcardDecks.FirstOrDefaultAsync(
            d => d.Id == request.DeckId && d.UserId == request.UserId, cancellationToken);

        if (deck is null)
        {
            return Result.Failure("Deck not found.", ResultErrorType.NotFound);
        }

        deck.ShareToken = null;
        deck.UpdatedAtUtc = _dateTimeProvider.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
