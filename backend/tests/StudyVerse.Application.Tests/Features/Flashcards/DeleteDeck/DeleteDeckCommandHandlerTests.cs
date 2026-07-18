using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.DeleteDeck;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.Features.Flashcards.DeleteDeck;

public sealed class DeleteDeckCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();

    private DeleteDeckCommandHandler CreateHandler() => new(_db);

    private FlashcardDeck SeedDeckWithCards(Guid ownerId, int cardCount)
    {
        var now = DateTime.UtcNow;
        var deck = new FlashcardDeck { Id = Guid.NewGuid(), UserId = ownerId, Title = "Deck", CreatedAtUtc = now, UpdatedAtUtc = now };
        _db.FlashcardDecks.Add(deck);

        for (var i = 0; i < cardCount; i++)
        {
            _db.Flashcards.Add(new Flashcard
            {
                Id = Guid.NewGuid(),
                DeckId = deck.Id,
                FrontText = $"Front {i}",
                BackText = $"Back {i}",
                EaseFactor = Sm2Scheduler.InitialEaseFactor,
                NextReviewDateUtc = DateOnly.FromDateTime(now),
            });
        }

        _db.SaveChanges();
        return deck;
    }

    [Fact]
    public async Task Handle_OwnerDeletesTheirOwnDeck_CascadeDeletesItsCards()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeckWithCards(ownerId, cardCount: 3);

        var result = await CreateHandler().Handle(new DeleteDeckCommand(ownerId, deck.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _db.FlashcardDecks.Should().NotContain(d => d.Id == deck.Id);
        _db.Flashcards.Should().NotContain(c => c.DeckId == deck.Id);
    }

    [Fact]
    public async Task Handle_WhenDeckBelongsToAnotherUser_FailsWithNotFoundAndDeletesNothing()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeckWithCards(ownerId, cardCount: 2);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new DeleteDeckCommand(attackerId, deck.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        _db.FlashcardDecks.Should().Contain(d => d.Id == deck.Id);
        _db.Flashcards.Should().HaveCount(2);
    }
}
