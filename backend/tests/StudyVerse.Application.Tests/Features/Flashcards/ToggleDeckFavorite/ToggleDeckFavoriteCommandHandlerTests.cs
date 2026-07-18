using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.ToggleDeckFavorite;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.Flashcards.ToggleDeckFavorite;

public sealed class ToggleDeckFavoriteCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private ToggleDeckFavoriteCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

    private FlashcardDeck SeedDeck(Guid ownerId, bool isFavorite = false)
    {
        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Deck",
            IsFavorite = isFavorite,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.FlashcardDecks.Add(deck);
        _db.SaveChanges();
        return deck;
    }

    [Fact]
    public async Task Handle_TogglingATwiceReturnsToTheOriginalState()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeck(ownerId, isFavorite: false);
        var handler = CreateHandler();

        var first = await handler.Handle(new ToggleDeckFavoriteCommand(ownerId, deck.Id), CancellationToken.None);
        first.Value.Should().BeTrue();
        _db.FlashcardDecks.Single(d => d.Id == deck.Id).IsFavorite.Should().BeTrue();

        var second = await handler.Handle(new ToggleDeckFavoriteCommand(ownerId, deck.Id), CancellationToken.None);
        second.Value.Should().BeFalse();
        _db.FlashcardDecks.Single(d => d.Id == deck.Id).IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenDeckBelongsToAnotherUser_FailsWithNotFound()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeck(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new ToggleDeckFavoriteCommand(attackerId, deck.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
