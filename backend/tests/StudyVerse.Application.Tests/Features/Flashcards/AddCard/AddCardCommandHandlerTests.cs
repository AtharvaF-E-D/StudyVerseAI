using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.AddCard;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.Features.Flashcards.AddCard;

public sealed class AddCardCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private AddCardCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

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
    public async Task Handle_OwnerAddsACardToTheirDeck_CreatesItWithFreshSm2SchedulingState()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeck(ownerId);

        var result = await CreateHandler().Handle(
            new AddCardCommand(ownerId, deck.Id, "Bonjour", "Hello", "https://example.com/img.png"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var card = _db.Flashcards.Single(c => c.Id == result.Value);
        card.FrontText.Should().Be("Bonjour");
        card.BackText.Should().Be("Hello");
        card.ImageUrl.Should().Be("https://example.com/img.png");
        card.EaseFactor.Should().Be(Sm2Scheduler.InitialEaseFactor);
        card.IntervalDays.Should().Be(0);
        card.Repetitions.Should().Be(0);
        card.NextReviewDateUtc.Should().Be(DateOnly.FromDateTime(_dateTimeProvider.UtcNow));
        card.LastReviewedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenDeckBelongsToAnotherUser_FailsWithNotFoundAndAddsNoCard()
    {
        var ownerId = Guid.NewGuid();
        var deck = SeedDeck(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(
            new AddCardCommand(attackerId, deck.Id, "Front", "Back", null), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        _db.Flashcards.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDeckDoesNotExist_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(
            new AddCardCommand(Guid.NewGuid(), Guid.NewGuid(), "Front", "Back", null), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
