using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.GetDeck;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.Features.Flashcards.GetDeck;

public sealed class GetDeckQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();

    private GetDeckQueryHandler CreateHandler() => new(_db);

    private FlashcardDeck SeedDeckWithCard(Guid ownerId)
    {
        var now = DateTime.UtcNow;
        var deck = new FlashcardDeck { Id = Guid.NewGuid(), UserId = ownerId, Title = "Deck", CreatedAtUtc = now, UpdatedAtUtc = now };
        _db.FlashcardDecks.Add(deck);
        _db.Flashcards.Add(new Flashcard
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            FrontText = "Front",
            BackText = "Back",
            EaseFactor = Sm2Scheduler.InitialEaseFactor,
            NextReviewDateUtc = DateOnly.FromDateTime(now),
        });
        _db.SaveChanges();
        return deck;
    }

    [Fact]
    public async Task Handle_OwnerRequestsTheirOwnDeck_ReturnsItWithAllCards()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeckWithCard(ownerId);

        var result = await CreateHandler().Handle(new GetDeckQuery(ownerId, deck.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(deck.Id);
        result.Value.Cards.Should().HaveCount(1);
        result.Value.Cards[0].FrontText.Should().Be("Front");
    }

    [Fact]
    public async Task Handle_WhenDeckBelongsToAnotherUser_FailsWithNotFound()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeckWithCard(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new GetDeckQuery(attackerId, deck.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenDeckDoesNotExist_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(new GetDeckQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
