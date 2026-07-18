using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.GetFlashcardStats;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.Features.Flashcards.GetFlashcardStats;

public sealed class GetFlashcardStatsQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 8, 0, 0, DateTimeKind.Utc) };

    private GetFlashcardStatsQueryHandler CreateHandler() => new(_db, _dateTimeProvider);

    [Fact]
    public async Task Handle_AggregatesDecksCardsDueTodayAndMasteredAcrossAllOfTheUsersDecks()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var now = _dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var deckA = new FlashcardDeck { Id = Guid.NewGuid(), UserId = userId, Title = "A", CreatedAtUtc = now, UpdatedAtUtc = now };
        var deckB = new FlashcardDeck { Id = Guid.NewGuid(), UserId = userId, Title = "B", CreatedAtUtc = now, UpdatedAtUtc = now };
        var otherDeck = new FlashcardDeck { Id = Guid.NewGuid(), UserId = otherUserId, Title = "Not mine", CreatedAtUtc = now, UpdatedAtUtc = now };
        _db.FlashcardDecks.AddRange(deckA, deckB, otherDeck);

        _db.Flashcards.AddRange(
            // Deck A: one due today, one mastered (repetitions >= 3), one neither.
            new Flashcard { Id = Guid.NewGuid(), DeckId = deckA.Id, FrontText = "F1", BackText = "B1", EaseFactor = Sm2Scheduler.InitialEaseFactor, NextReviewDateUtc = today, Repetitions = 0 },
            new Flashcard { Id = Guid.NewGuid(), DeckId = deckA.Id, FrontText = "F2", BackText = "B2", EaseFactor = Sm2Scheduler.InitialEaseFactor, NextReviewDateUtc = today.AddDays(20), Repetitions = 3 },
            new Flashcard { Id = Guid.NewGuid(), DeckId = deckA.Id, FrontText = "F3", BackText = "B3", EaseFactor = Sm2Scheduler.InitialEaseFactor, NextReviewDateUtc = today.AddDays(20), Repetitions = 1 },
            // Deck B: one overdue (still counts as "due today").
            new Flashcard { Id = Guid.NewGuid(), DeckId = deckB.Id, FrontText = "F4", BackText = "B4", EaseFactor = Sm2Scheduler.InitialEaseFactor, NextReviewDateUtc = today.AddDays(-2), Repetitions = 5 },
            // Someone else's card must never be counted.
            new Flashcard { Id = Guid.NewGuid(), DeckId = otherDeck.Id, FrontText = "F5", BackText = "B5", EaseFactor = Sm2Scheduler.InitialEaseFactor, NextReviewDateUtc = today, Repetitions = 10 });

        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetFlashcardStatsQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalDecks.Should().Be(2);
        result.Value.TotalCards.Should().Be(4);
        result.Value.CardsDueToday.Should().Be(2); // F1 (today) + F4 (overdue).
        result.Value.MasteredCardCount.Should().Be(2); // F2 (reps=3) + F4 (reps=5).
    }

    [Fact]
    public async Task Handle_UserWithNoDecks_ReturnsAllZeroes()
    {
        var result = await CreateHandler().Handle(new GetFlashcardStatsQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new Application.Features.Flashcards.Common.FlashcardStatsDto(0, 0, 0, 0));
    }
}
