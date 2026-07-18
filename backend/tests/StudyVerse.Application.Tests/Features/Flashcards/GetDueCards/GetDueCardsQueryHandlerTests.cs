using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.GetDueCards;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.Features.Flashcards.GetDueCards;

public sealed class GetDueCardsQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 9, 0, 0, DateTimeKind.Utc) };
    private DateOnly Today => DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

    private GetDueCardsQueryHandler CreateHandler() => new(_db, _dateTimeProvider);

    private Flashcard MakeCard(Guid deckId, string front, DateOnly nextReviewDate) => new()
    {
        Id = Guid.NewGuid(),
        DeckId = deckId,
        FrontText = front,
        BackText = $"{front} answer",
        EaseFactor = Sm2Scheduler.InitialEaseFactor,
        NextReviewDateUtc = nextReviewDate,
    };

    [Fact]
    public async Task Handle_NoDeckFilter_ReturnsOnlyDueCardsAcrossAllOfTheUsersDecksNotFutureOnesOrOtherUsers()
    {
        var userId = Guid.NewGuid();
        var now = _dateTimeProvider.UtcNow;

        var deckA = new FlashcardDeck { Id = Guid.NewGuid(), UserId = userId, Title = "Deck A", CreatedAtUtc = now, UpdatedAtUtc = now };
        var deckB = new FlashcardDeck { Id = Guid.NewGuid(), UserId = userId, Title = "Deck B", CreatedAtUtc = now, UpdatedAtUtc = now };
        var otherUsersDeck = new FlashcardDeck { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Title = "Not mine", CreatedAtUtc = now, UpdatedAtUtc = now };
        _db.FlashcardDecks.AddRange(deckA, deckB, otherUsersDeck);

        _db.Flashcards.AddRange(
            MakeCard(deckA.Id, "DueToday", Today),
            MakeCard(deckA.Id, "Overdue", Today.AddDays(-3)),
            MakeCard(deckB.Id, "DueTodayOtherDeck", Today),
            MakeCard(deckB.Id, "NotYetDue", Today.AddDays(2)),
            MakeCard(otherUsersDeck.Id, "SomeoneElsesDueCard", Today));

        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetDueCardsQuery(userId, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(c => c.FrontText).Should().BeEquivalentTo(["DueToday", "Overdue", "DueTodayOtherDeck"]);
    }

    [Fact]
    public async Task Handle_ScopedToOneDeck_ReturnsOnlyThatDecksDueCards()
    {
        var userId = Guid.NewGuid();
        var now = _dateTimeProvider.UtcNow;

        var deckA = new FlashcardDeck { Id = Guid.NewGuid(), UserId = userId, Title = "Deck A", CreatedAtUtc = now, UpdatedAtUtc = now };
        var deckB = new FlashcardDeck { Id = Guid.NewGuid(), UserId = userId, Title = "Deck B", CreatedAtUtc = now, UpdatedAtUtc = now };
        _db.FlashcardDecks.AddRange(deckA, deckB);
        _db.Flashcards.AddRange(
            MakeCard(deckA.Id, "InDeckA", Today),
            MakeCard(deckB.Id, "InDeckB", Today));
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetDueCardsQuery(userId, deckA.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(c => c.FrontText == "InDeckA");
    }

    [Fact]
    public async Task Handle_JustReviewedCardWithAFutureNextReviewDate_IsExcludedFromTodaysQueue()
    {
        var userId = Guid.NewGuid();
        var now = _dateTimeProvider.UtcNow;
        var deck = new FlashcardDeck { Id = Guid.NewGuid(), UserId = userId, Title = "Deck", CreatedAtUtc = now, UpdatedAtUtc = now };
        _db.FlashcardDecks.Add(deck);
        // Simulates a card that was just reviewed with a Good rating, pushing it to tomorrow.
        _db.Flashcards.Add(MakeCard(deck.Id, "JustReviewed", Today.AddDays(1)));
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetDueCardsQuery(userId, null), CancellationToken.None);

        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DeckIdForAnotherUsersDeck_FailsWithNotFound()
    {
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var now = _dateTimeProvider.UtcNow;
        var deck = new FlashcardDeck { Id = Guid.NewGuid(), UserId = ownerId, Title = "Deck", CreatedAtUtc = now, UpdatedAtUtc = now };
        _db.FlashcardDecks.Add(deck);
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetDueCardsQuery(attackerId, deck.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
