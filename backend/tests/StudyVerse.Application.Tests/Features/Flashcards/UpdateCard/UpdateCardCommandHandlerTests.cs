using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.UpdateCard;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.Features.Flashcards.UpdateCard;

public sealed class UpdateCardCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private UpdateCardCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

    private (FlashcardDeck Deck, Flashcard Card) SeedDeckWithCard(Guid ownerId)
    {
        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Deck",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        var card = new Flashcard
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            FrontText = "Old front",
            BackText = "Old back",
            EaseFactor = Sm2Scheduler.InitialEaseFactor,
            NextReviewDateUtc = DateOnly.FromDateTime(_dateTimeProvider.UtcNow),
        };
        _db.FlashcardDecks.Add(deck);
        _db.Flashcards.Add(card);
        _db.SaveChanges();
        return (deck, card);
    }

    [Fact]
    public async Task Handle_OwnerUpdatesTheirOwnCard_PersistsTheNewFrontBackAndImage()
    {
        var ownerId = Guid.NewGuid();
        var (deck, card) = SeedDeckWithCard(ownerId);

        var result = await CreateHandler().Handle(
            new UpdateCardCommand(ownerId, deck.Id, card.Id, "New front", "New back", "https://example.com/new.png"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var updated = _db.Flashcards.Single(c => c.Id == card.Id);
        updated.FrontText.Should().Be("New front");
        updated.BackText.Should().Be("New back");
        updated.ImageUrl.Should().Be("https://example.com/new.png");
    }

    [Fact]
    public async Task Handle_WhenCardBelongsToAnotherUsersDeck_FailsWithNotFoundAndDoesNotMutateTheCard()
    {
        var ownerId = Guid.NewGuid();
        var (deck, card) = SeedDeckWithCard(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(
            new UpdateCardCommand(attackerId, deck.Id, card.Id, "Hacked front", "Hacked back", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);

        var unchanged = _db.Flashcards.Single(c => c.Id == card.Id);
        unchanged.FrontText.Should().Be("Old front");
    }

    [Fact]
    public async Task Handle_WhenCardIdDoesNotBelongToTheGivenDeck_FailsWithNotFound()
    {
        var ownerId = Guid.NewGuid();
        var (deck, card) = SeedDeckWithCard(ownerId);
        var otherDeck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Other deck",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.FlashcardDecks.Add(otherDeck);
        _db.SaveChanges();

        var result = await CreateHandler().Handle(
            new UpdateCardCommand(ownerId, otherDeck.Id, card.Id, "Front", "Back", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
