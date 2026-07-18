using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.CreateDeck;
using StudyVerse.Application.Tests.TestSupport;

namespace StudyVerse.Application.Tests.Features.Flashcards.CreateDeck;

public sealed class CreateDeckCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private CreateDeckCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

    [Fact]
    public async Task Handle_ValidCommand_CreatesAnEmptyDeckOwnedByTheCaller()
    {
        var userId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new CreateDeckCommand(userId, "French Verbs", "Common irregular verbs"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var deck = _db.FlashcardDecks.Single(d => d.Id == result.Value);
        deck.UserId.Should().Be(userId);
        deck.Title.Should().Be("French Verbs");
        deck.Description.Should().Be("Common irregular verbs");
        deck.IsFavorite.Should().BeFalse();
        deck.ShareToken.Should().BeNull();
        deck.SourceNoteId.Should().BeNull();
        deck.CreatedAtUtc.Should().Be(_dateTimeProvider.UtcNow);

        _db.Flashcards.Where(c => c.DeckId == deck.Id).Should().BeEmpty();
    }
}
