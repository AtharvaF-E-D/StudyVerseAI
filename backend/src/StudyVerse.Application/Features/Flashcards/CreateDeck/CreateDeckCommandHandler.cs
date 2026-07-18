using MediatR;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Features.Flashcards.CreateDeck;

public sealed class CreateDeckCommandHandler : IRequestHandler<CreateDeckCommand, Result<Guid>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateDeckCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(CreateDeckCommand request, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;
        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Description = request.Description,
            IsFavorite = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        _db.FlashcardDecks.Add(deck);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(deck.Id);
    }
}
