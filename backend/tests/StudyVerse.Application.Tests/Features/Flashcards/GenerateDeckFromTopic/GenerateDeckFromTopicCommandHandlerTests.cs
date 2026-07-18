using FluentAssertions;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Flashcards.GenerateDeckFromTopic;
using StudyVerse.Application.Tests.TestSupport;

namespace StudyVerse.Application.Tests.Features.Flashcards.GenerateDeckFromTopic;

public sealed class GenerateDeckFromTopicCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();
    private readonly IFlashcardGenerationProvider _flashcardGenerationProvider = Substitute.For<IFlashcardGenerationProvider>();

    private GenerateDeckFromTopicCommandHandler CreateHandler() =>
        new(_db, _flashcardGenerationProvider, _dateTimeProvider);

    [Fact]
    public async Task Handle_ProviderReturnsCards_CreatesTheDeckAndAllGeneratedCardsWithFreshSm2State()
    {
        var userId = Guid.NewGuid();
        _flashcardGenerationProvider
            .GenerateFlashcardsAsync("Basic French Greetings", 3, Arg.Any<CancellationToken>())
            .Returns(new List<(string Front, string Back)>
            {
                ("Bonjour", "Hello"),
                ("Merci", "Thank you"),
                ("Au revoir", "Goodbye"),
            });

        var result = await CreateHandler().Handle(
            new GenerateDeckFromTopicCommand(userId, "French Greetings", "Basic French Greetings", 3),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var deck = _db.FlashcardDecks.Single(d => d.Id == result.Value);
        deck.UserId.Should().Be(userId);
        deck.Title.Should().Be("French Greetings");
        deck.SourceNoteId.Should().BeNull();

        var cards = _db.Flashcards.Where(c => c.DeckId == deck.Id).ToList();
        cards.Should().HaveCount(3);
        cards.Should().Contain(c => c.FrontText == "Bonjour" && c.BackText == "Hello");
        cards.Should().OnlyContain(c => c.Repetitions == 0 && c.IntervalDays == 0);
    }

    [Fact]
    public async Task Handle_ProviderReturnsNoCards_FailsAndCreatesNoDeck()
    {
        _flashcardGenerationProvider
            .GenerateFlashcardsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<(string Front, string Back)>());

        var result = await CreateHandler().Handle(
            new GenerateDeckFromTopicCommand(Guid.NewGuid(), "Title", "Some obscure topic", 5),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _db.FlashcardDecks.Should().BeEmpty();
    }
}
