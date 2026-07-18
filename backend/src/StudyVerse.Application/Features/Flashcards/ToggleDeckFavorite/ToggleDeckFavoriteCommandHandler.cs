using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.ToggleDeckFavorite;

/// <summary>Flips <c>IsFavorite</c> and returns the new value, so the client doesn't need a
/// separate re-fetch to know which way the toggle landed.</summary>
public sealed class ToggleDeckFavoriteCommandHandler : IRequestHandler<ToggleDeckFavoriteCommand, Result<bool>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ToggleDeckFavoriteCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<bool>> Handle(ToggleDeckFavoriteCommand request, CancellationToken cancellationToken)
    {
        var deck = await _db.FlashcardDecks.FirstOrDefaultAsync(
            d => d.Id == request.DeckId && d.UserId == request.UserId, cancellationToken);

        if (deck is null)
        {
            return Result.Failure<bool>("Deck not found.", ResultErrorType.NotFound);
        }

        deck.IsFavorite = !deck.IsFavorite;
        deck.UpdatedAtUtc = _dateTimeProvider.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(deck.IsFavorite);
    }
}
