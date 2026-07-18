using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.DeleteCard;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.Features.Flashcards.DeleteCard;

public sealed class DeleteCardCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();

    private DeleteCardCommandHandler CreateHandler() => new(_db);

    private (FlashcardDeck Deck, Flashcard Card) SeedDeckWithCard(Guid ownerId)
    {
        var now = DateTime.UtcNow;
        var deck = new FlashcardDeck { Id = Guid.NewGuid(), UserId = ownerId, Title = "Deck", CreatedAtUtc = now, UpdatedAtUtc = now };
        var card = new Flashcard
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            FrontText = "Front",
            BackText = "Back",
            EaseFactor = Sm2Scheduler.InitialEaseFactor,
            NextReviewDateUtc = DateOnly.FromDateTime(now),
        };
        _db.FlashcardDecks.Add(deck);
        _db.Flashcards.Add(card);
        _db.SaveChanges();
        return (deck, card);
    }

    [Fact]
    public async Task Handle_OwnerDeletesTheirOwnCard_RemovesIt()
    {
        var ownerId = Guid.NewGuid();
        var (deck, card) = SeedDeckWithCard(ownerId);

        var result = await CreateHandler().Handle(new DeleteCardCommand(ownerId, deck.Id, card.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _db.Flashcards.Should().NotContain(c => c.Id == card.Id);
    }

    [Fact]
    public async Task Handle_WhenCardBelongsToAnotherUsersDeck_FailsWithNotFoundAndDoesNotDeleteIt()
    {
        var ownerId = Guid.NewGuid();
        var (deck, card) = SeedDeckWithCard(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new DeleteCardCommand(attackerId, deck.Id, card.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        _db.Flashcards.Should().Contain(c => c.Id == card.Id);
    }
}
