using FluentAssertions;
using StudyVerse.Application.Features.Notes.GetNote;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.Notes.GetNote;

public sealed class GetNoteQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();

    private GetNoteQueryHandler CreateHandler() => new(_db);

    private Note SeedReadyNote(Guid ownerId)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Water cycle",
            SourceFileName = "water-cycle.pdf",
            SourceFileType = NoteSourceFileType.Pdf,
            StorageKey = "abc123.pdf",
            ExtractedText = "The water cycle describes how water moves through the earth's systems.",
            Status = NoteStatus.Ready,
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        _db.Notes.Add(note);

        _db.NoteContents.Add(new NoteContent
        {
            Id = Guid.NewGuid(),
            NoteId = note.Id,
            Summary = "A summary of the water cycle.",
            KeyPointsJson = "[\"Evaporation\",\"Condensation\"]",
            FlashcardsJson = "[]",
            McqsJson = "[]",
            MindMapJson = "{\"topic\":\"Water cycle\",\"children\":[]}",
            RevisionSheet = "# Water cycle",
            VocabularyJson = "[]",
            FormulasJson = "[]",
        });

        _db.SaveChanges();
        return note;
    }

    [Fact]
    public async Task Handle_OwnerRequestsTheirOwnReadyNote_ReturnsFullNoteWithContent()
    {
        var ownerId = Guid.NewGuid();
        var note = SeedReadyNote(ownerId);

        var result = await CreateHandler().Handle(new GetNoteQuery(ownerId, note.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(note.Id);
        result.Value.Status.Should().Be(NoteStatus.Ready);
        result.Value.Content.Should().NotBeNull();
        result.Value.Content!.Summary.Should().Be("A summary of the water cycle.");
        result.Value.Content.KeyPoints.Should().BeEquivalentTo(["Evaporation", "Condensation"]);
    }

    [Fact]
    public async Task Handle_WhenNoteBelongsToAnotherUser_FailsWithNotFound()
    {
        var ownerId = Guid.NewGuid();
        var note = SeedReadyNote(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new GetNoteQuery(attackerId, note.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNoteDoesNotExist_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(new GetNoteQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNoteIsStillProcessing_ReturnsNullContent()
    {
        var ownerId = Guid.NewGuid();
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Untitled note",
            SourceFileName = "upload.pdf",
            SourceFileType = NoteSourceFileType.Pdf,
            StorageKey = "key.pdf",
            ExtractedText = string.Empty,
            Status = NoteStatus.Processing,
            CreatedAtUtc = DateTime.UtcNow,
        };
        _db.Notes.Add(note);
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetNoteQuery(ownerId, note.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(NoteStatus.Processing);
        result.Value.Content.Should().BeNull();
    }
}
