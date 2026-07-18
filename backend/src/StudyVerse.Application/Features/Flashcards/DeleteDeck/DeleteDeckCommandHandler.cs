using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.DeleteDeck;

public sealed class DeleteDeckCommandHandler : IRequestHandler<DeleteDeckCommand, Result>
{
    private readonly IAppDbContext _db;

    public DeleteDeckCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeleteDeckCommand request, CancellationToken cancellationToken)
    {
        var deck = await _db.FlashcardDecks.FirstOrDefaultAsync(
            d => d.Id == request.DeckId && d.UserId == request.UserId, cancellationToken);

        if (deck is null)
        {
            return Result.Failure("Deck not found.", ResultErrorType.NotFound);
        }

        _db.FlashcardDecks.Remove(deck);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
