using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.ReviewCard;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.Features.Flashcards.ReviewCard;

public sealed class ReviewCardCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 14, 30, 0, DateTimeKind.Utc) };

    private ReviewCardCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

    private (FlashcardDeck Deck, Flashcard Card) SeedDeckWithFreshCard(Guid ownerId)
    {
        var now = _dateTimeProvider.UtcNow;
        var deck = new FlashcardDeck { Id = Guid.NewGuid(), UserId = ownerId, Title = "Deck", CreatedAtUtc = now, UpdatedAtUtc = now };
        var card = new Flashcard
        {
            Id = Guid.NewGuid(),
            DeckId = deck.Id,
            FrontText = "Front",
            BackText = "Back",
            EaseFactor = Sm2Scheduler.InitialEaseFactor,
            IntervalDays = 0,
            Repetitions = 0,
            NextReviewDateUtc = DateOnly.FromDateTime(now),
        };
        _db.FlashcardDecks.Add(deck);
        _db.Flashcards.Add(card);
        _db.SaveChanges();
        return (deck, card);
    }

    [Fact]
    public async Task Handle_GoodReviewOnAFreshCard_AppliesSm2AndStampsLastReviewedAtUtc()
    {
        var ownerId = Guid.NewGuid();
        var (_, card) = SeedDeckWithFreshCard(ownerId);

        var result = await CreateHandler().Handle(new ReviewCardCommand(ownerId, card.Id, ReviewQuality.Good), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IntervalDays.Should().Be(1);
        result.Value.Repetitions.Should().Be(1);
        result.Value.EaseFactor.Should().BeApproximately(2.5, 0.0001);
        result.Value.NextReviewDateUtc.Should().Be(DateOnly.FromDateTime(_dateTimeProvider.UtcNow).AddDays(1));
        result.Value.LastReviewedAtUtc.Should().Be(_dateTimeProvider.UtcNow);

        var persisted = _db.Flashcards.Single(c => c.Id == card.Id);
        persisted.IntervalDays.Should().Be(1);
        persisted.Repetitions.Should().Be(1);
        persisted.LastReviewedAtUtc.Should().Be(_dateTimeProvider.UtcNow);
    }

    [Fact]
    public async Task Handle_ThreeConsecutiveGoodReviews_ProgressesIntervalsOneSixFifteen()
    {
        var ownerId = Guid.NewGuid();
        var (_, card) = SeedDeckWithFreshCard(ownerId);
        var handler = CreateHandler();

        var first = await handler.Handle(new ReviewCardCommand(ownerId, card.Id, ReviewQuality.Good), CancellationToken.None);
        first.Value.IntervalDays.Should().Be(1);

        var second = await handler.Handle(new ReviewCardCommand(ownerId, card.Id, ReviewQuality.Good), CancellationToken.None);
        second.Value.IntervalDays.Should().Be(6);

        var third = await handler.Handle(new ReviewCardCommand(ownerId, card.Id, ReviewQuality.Good), CancellationToken.None);
        third.Value.IntervalDays.Should().Be(15);
    }

    [Fact]
    public async Task Handle_AgainAfterAGoodStreak_ResetsIntervalAndRepetitionsOnThePersistedCard()
    {
        var ownerId = Guid.NewGuid();
        var (_, card) = SeedDeckWithFreshCard(ownerId);
        var handler = CreateHandler();

        await handler.Handle(new ReviewCardCommand(ownerId, card.Id, ReviewQuality.Good), CancellationToken.None);
        await handler.Handle(new ReviewCardCommand(ownerId, card.Id, ReviewQuality.Good), CancellationToken.None);

        var afterAgain = await handler.Handle(new ReviewCardCommand(ownerId, card.Id, ReviewQuality.Again), CancellationToken.None);

        afterAgain.Value.Repetitions.Should().Be(0);
        afterAgain.Value.IntervalDays.Should().Be(1);
        afterAgain.Value.EaseFactor.Should().BeGreaterThan(Sm2Scheduler.MinEaseFactor);
    }

    [Fact]
    public async Task Handle_WhenCardBelongsToAnotherUsersDeck_FailsWithNotFoundAndDoesNotMutateTheCard()
    {
        var ownerId = Guid.NewGuid();
        var (_, card) = SeedDeckWithFreshCard(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new ReviewCardCommand(attackerId, card.Id, ReviewQuality.Easy), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);

        var unchanged = _db.Flashcards.Single(c => c.Id == card.Id);
        unchanged.Repetitions.Should().Be(0);
        unchanged.LastReviewedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCardDoesNotExist_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(
            new ReviewCardCommand(Guid.NewGuid(), Guid.NewGuid(), ReviewQuality.Good), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
