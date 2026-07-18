using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.UnshareDeck;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.Flashcards.UnshareDeck;

public sealed class UnshareDeckCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private UnshareDeckCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

    private FlashcardDeck SeedSharedDeck(Guid ownerId)
    {
        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Deck",
            ShareToken = "existing-token",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.FlashcardDecks.Add(deck);
        _db.SaveChanges();
        return deck;
    }

    [Fact]
    public async Task Handle_OwnerUnsharesTheirDeck_ClearsTheShareToken()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedSharedDeck(ownerId);

        var result = await CreateHandler().Handle(new UnshareDeckCommand(ownerId, deck.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _db.FlashcardDecks.Single(d => d.Id == deck.Id).ShareToken.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenDeckBelongsToAnotherUser_FailsWithNotFoundAndLeavesTheTokenIntact()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedSharedDeck(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new UnshareDeckCommand(attackerId, deck.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        _db.FlashcardDecks.Single(d => d.Id == deck.Id).ShareToken.Should().Be("existing-token");
    }
}
