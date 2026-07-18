using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Features.Flashcards.AddCard;

public sealed class AddCardCommandHandler : IRequestHandler<AddCardCommand, Result<Guid>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AddCardCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(AddCardCommand request, CancellationToken cancellationToken)
    {
        var deck = await _db.FlashcardDecks.FirstOrDefaultAsync(
            d => d.Id == request.DeckId && d.UserId == request.UserId, cancellationToken);

        if (deck is null)
        {
            return Result.Failure<Guid>("Deck not found.", ResultErrorType.NotFound);
        }

        var now = _dateTimeProvider.UtcNow;
        var card = new Flashcard
        {
            Id = Guid.NewGuid(),
            DeckId = request.DeckId,
            FrontText = request.FrontText,
            BackText = request.BackText,
            ImageUrl = request.ImageUrl,
            EaseFactor = Sm2Scheduler.InitialEaseFactor,
            IntervalDays = 0,
            Repetitions = 0,
            NextReviewDateUtc = DateOnly.FromDateTime(now),
        };
        _db.Flashcards.Add(card);
        deck.UpdatedAtUtc = now;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(card.Id);
    }
}
