using FluentAssertions;
using StudyVerse.Application.Features.Flashcards.GenerateDeckFromNote;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.Flashcards.GenerateDeckFromNote;

public sealed class GenerateDeckFromNoteCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GenerateDeckFromNoteCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

    private Note SeedNote(Guid ownerId, NoteStatus status, string flashcardsJson)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Water Cycle",
            SourceFileName = "water-cycle.pdf",
            SourceFileType = NoteSourceFileType.Pdf,
            StorageKey = "key.pdf",
            ExtractedText = "text",
            Status = status,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Notes.Add(note);

        if (status == NoteStatus.Ready)
        {
            _db.NoteContents.Add(new NoteContent
            {
                Id = Guid.NewGuid(),
                NoteId = note.Id,
                Summary = "summary",
                KeyPointsJson = "[]",
                FlashcardsJson = flashcardsJson,
                McqsJson = "[]",
                MindMapJson = "{}",
                RevisionSheet = "sheet",
                VocabularyJson = "[]",
                FormulasJson = "[]",
            });
        }

        _db.SaveChanges();
        return note;
    }

    [Fact]
    public async Task Handle_ReadyNoteWithFlashcards_CreatesADeckWithSourceNoteIdCopyingEachCard()
    {
        var ownerId = Guid.NewGuid();
        var note = SeedNote(ownerId, NoteStatus.Ready,
            """[{"question":"What is evaporation?","answer":"Water turning into vapor"},{"question":"What is condensation?","answer":"Vapor turning into water"}]""");

        var result = await CreateHandler().Handle(new GenerateDeckFromNoteCommand(ownerId, note.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var deck = _db.FlashcardDecks.Single(d => d.Id == result.Value);
        deck.UserId.Should().Be(ownerId);
        deck.Title.Should().Be("Water Cycle");
        deck.SourceNoteId.Should().Be(note.Id);

        var cards = _db.Flashcards.Where(c => c.DeckId == deck.Id).ToList();
        cards.Should().HaveCount(2);
        cards.Should().Contain(c => c.FrontText == "What is evaporation?" && c.BackText == "Water turning into vapor");
        cards.Should().Contain(c => c.FrontText == "What is condensation?" && c.BackText == "Vapor turning into water");
    }

    [Fact]
    public async Task Handle_NoteWithNoGeneratedFlashcards_FailsAndCreatesNoDeck()
    {
        var ownerId = Guid.NewGuid();
        var note = SeedNote(ownerId, NoteStatus.Ready, "[]");

        var result = await CreateHandler().Handle(new GenerateDeckFromNoteCommand(ownerId, note.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _db.FlashcardDecks.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoteStillProcessing_FailsAndCreatesNoDeck()
    {
        var ownerId = Guid.NewGuid();
        var note = SeedNote(ownerId, NoteStatus.Processing, "[]");

        var result = await CreateHandler().Handle(new GenerateDeckFromNoteCommand(ownerId, note.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _db.FlashcardDecks.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoteBelongingToAnotherUser_FailsWithNotFound()
    {
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var note = SeedNote(ownerId, NoteStatus.Ready, """[{"question":"Q","answer":"A"}]""");

        var result = await CreateHandler().Handle(new GenerateDeckFromNoteCommand(attackerId, note.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        _db.FlashcardDecks.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoteThatDoesNotExist_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(
            new GenerateDeckFromNoteCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
