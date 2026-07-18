using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.GetDecks;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.Features.Flashcards.GetDecks;

public sealed class GetDecksQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 12, 0, 0, DateTimeKind.Utc) };

    private GetDecksQueryHandler CreateHandler() => new(_db, _dateTimeProvider);

    [Fact]
    public async Task Handle_ReturnsOnlyTheCallersDecksWithCorrectCountsAndSharedFlag()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "My Deck",
            ShareToken = "shared-token",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.FlashcardDecks.Add(deck);

        // 2 cards due today or earlier, 1 card due in the future.
        _db.Flashcards.AddRange(
            new Flashcard { Id = Guid.NewGuid(), DeckId = deck.Id, FrontText = "F1", BackText = "B1", EaseFactor = Sm2Scheduler.InitialEaseFactor, NextReviewDateUtc = today },
            new Flashcard { Id = Guid.NewGuid(), DeckId = deck.Id, FrontText = "F2", BackText = "B2", EaseFactor = Sm2Scheduler.InitialEaseFactor, NextReviewDateUtc = today.AddDays(-1) },
            new Flashcard { Id = Guid.NewGuid(), DeckId = deck.Id, FrontText = "F3", BackText = "B3", EaseFactor = Sm2Scheduler.InitialEaseFactor, NextReviewDateUtc = today.AddDays(5) });

        // A different user's deck must never show up in this list.
        _db.FlashcardDecks.Add(new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            Title = "Someone else's deck",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        });

        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetDecksQuery(ownerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var summary = result.Value.Single();
        summary.Id.Should().Be(deck.Id);
        summary.CardCount.Should().Be(3);
        summary.DueTodayCount.Should().Be(2);
        summary.IsShared.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserWithNoDecks_ReturnsAnEmptyList()
    {
        var result = await CreateHandler().Handle(new GetDecksQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
