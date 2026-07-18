using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.UpdateCard;

public sealed class UpdateCardCommandHandler : IRequestHandler<UpdateCardCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateCardCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(UpdateCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _db.Flashcards.FirstOrDefaultAsync(
            c => c.Id == request.CardId
                && c.DeckId == request.DeckId
                && c.Deck!.UserId == request.UserId,
            cancellationToken);

        if (card is null)
        {
            return Result.Failure("Card not found.", ResultErrorType.NotFound);
        }

        card.FrontText = request.FrontText;
        card.BackText = request.BackText;
        card.ImageUrl = request.ImageUrl;

        var now = _dateTimeProvider.UtcNow;
        var deck = await _db.FlashcardDecks.FirstAsync(d => d.Id == request.DeckId, cancellationToken);
        deck.UpdatedAtUtc = now;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
