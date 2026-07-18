using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.GetSharedDeck;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Tests.Features.Flashcards.GetSharedDeck;

/// <summary>Exercises the one Flashcards query with no auth/ownership check at all - resolved
/// purely by the public share token (see <see cref="FlashcardDeck.ShareToken"/>'s doc comment).</summary>
public sealed class GetSharedDeckQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();

    private GetSharedDeckQueryHandler CreateHandler() => new(_db);

    [Fact]
    public async Task Handle_AValidShareToken_ReturnsTheDeckWithNoUserIdOrAuthContextInvolvedAtAll()
    {
        var now = DateTime.UtcNow;
        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Shared Deck",
            Description = "A deck anyone with the link can view",
            ShareToken = "abc123token",
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
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

        // Note: this query object carries no UserId at all - there is nothing to fake being
        // "unauthenticated" here, the type itself has no auth-shaped field to check.
        var result = await CreateHandler().Handle(new GetSharedDeckQuery("abc123token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Shared Deck");
        result.Value.Cards.Should().ContainSingle(c => c.FrontText == "Front" && c.BackText == "Back");
    }

    [Fact]
    public async Task Handle_ATokenThatDoesNotExist_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(new GetSharedDeckQuery("no-such-token"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ADeckThatWasUnsharedSinceTheLinkWasIssued_FailsWithNotFound()
    {
        var now = DateTime.UtcNow;
        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Once shared",
            ShareToken = null, // unshared - the old token no longer resolves to anything.
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        _db.FlashcardDecks.Add(deck);
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetSharedDeckQuery("previously-issued-token"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
