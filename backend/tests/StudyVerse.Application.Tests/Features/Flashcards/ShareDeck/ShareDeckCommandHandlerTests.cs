using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.ShareDeck;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.Flashcards.ShareDeck;

public sealed class ShareDeckCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private ShareDeckCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

    private FlashcardDeck SeedDeck(Guid ownerId)
    {
        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Deck",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.FlashcardDecks.Add(deck);
        _db.SaveChanges();
        return deck;
    }

    [Fact]
    public async Task Handle_SharingAnUnsharedDeck_GeneratesANonEmptyToken()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeck(ownerId);

        var result = await CreateHandler().Handle(new ShareDeckCommand(ownerId, deck.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ShareToken.Should().NotBeNullOrWhiteSpace();
        _db.FlashcardDecks.Single(d => d.Id == deck.Id).ShareToken.Should().Be(result.Value.ShareToken);
    }

    [Fact]
    public async Task Handle_SharingAnAlreadySharedDeck_ReturnsTheSameExistingToken()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeck(ownerId);
        var handler = CreateHandler();

        var first = await handler.Handle(new ShareDeckCommand(ownerId, deck.Id), CancellationToken.None);
        var second = await handler.Handle(new ShareDeckCommand(ownerId, deck.Id), CancellationToken.None);

        second.Value.ShareToken.Should().Be(first.Value.ShareToken);
    }

    [Fact]
    public async Task Handle_WhenDeckBelongsToAnotherUser_FailsWithNotFound()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeck(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new ShareDeckCommand(attackerId, deck.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
