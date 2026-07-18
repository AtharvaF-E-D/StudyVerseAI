using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Security;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.ShareDeck;

public sealed class ShareDeckCommandHandler : IRequestHandler<ShareDeckCommand, Result<ShareDeckResultDto>>
{
    /// <summary>9 random bytes -> 12 URL-safe base64 characters: short enough for a clean share
    /// link, long enough that guessing a live token is infeasible.</summary>
    private const int ShareTokenByteLength = 9;

    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ShareDeckCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<ShareDeckResultDto>> Handle(ShareDeckCommand request, CancellationToken cancellationToken)
    {
        var deck = await _db.FlashcardDecks.FirstOrDefaultAsync(
            d => d.Id == request.DeckId && d.UserId == request.UserId, cancellationToken);

        if (deck is null)
        {
            return Result.Failure<ShareDeckResultDto>("Deck not found.", ResultErrorType.NotFound);
        }

        if (deck.ShareToken is null)
        {
            deck.ShareToken = SecureTokenGenerator.GenerateUrlSafeToken(ShareTokenByteLength);
            deck.UpdatedAtUtc = _dateTimeProvider.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new ShareDeckResultDto(deck.Id, deck.ShareToken));
    }
}
