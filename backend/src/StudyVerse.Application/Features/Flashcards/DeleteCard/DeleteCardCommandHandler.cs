using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.DeleteCard;

public sealed class DeleteCardCommandHandler : IRequestHandler<DeleteCardCommand, Result>
{
    private readonly IAppDbContext _db;

    public DeleteCardCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeleteCardCommand request, CancellationToken cancellationToken)
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

        _db.Flashcards.Remove(card);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
