using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Notes.DeleteNote;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.Notes.DeleteNote;

public sealed class DeleteNoteCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly IFileStorageService _fileStorage = Substitute.For<IFileStorageService>();

    private DeleteNoteCommandHandler CreateHandler() =>
        new(_db, _fileStorage, Substitute.For<ILogger<DeleteNoteCommandHandler>>());

    private Note SeedNote(Guid ownerId, string storageKey = "stored-file-key.pdf")
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = "Some note",
            SourceFileName = "some-note.pdf",
            SourceFileType = NoteSourceFileType.Pdf,
            StorageKey = storageKey,
            ExtractedText = "text",
            Status = NoteStatus.Ready,
            CreatedAtUtc = DateTime.UtcNow,
        };
        _db.Notes.Add(note);
        _db.NoteContents.Add(new NoteContent
        {
            Id = Guid.NewGuid(),
            NoteId = note.Id,
            Summary = "summary",
            KeyPointsJson = "[]",
            FlashcardsJson = "[]",
            McqsJson = "[]",
            MindMapJson = "{}",
            RevisionSheet = "sheet",
            VocabularyJson = "[]",
            FormulasJson = "[]",
        });
        _db.SaveChanges();
        return note;
    }

    [Fact]
    public async Task Handle_OwnerDeletesTheirOwnNote_RemovesDbRowsAndDeletesStoredFile()
    {
        var ownerId = Guid.NewGuid();
        var note = SeedNote(ownerId, storageKey: "the-stored-key.pdf");

        var result = await CreateHandler().Handle(new DeleteNoteCommand(ownerId, note.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _db.Notes.Should().NotContain(n => n.Id == note.Id);
        _db.NoteContents.Should().NotContain(c => c.NoteId == note.Id);

        await _fileStorage.Received(1).DeleteAsync("the-stored-key.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoteBelongsToAnotherUser_FailsWithNotFoundAndDoesNotDeleteAnything()
    {
        var ownerId = Guid.NewGuid();
        var note = SeedNote(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new DeleteNoteCommand(attackerId, note.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);

        _db.Notes.Should().Contain(n => n.Id == note.Id);
        await _fileStorage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenNoteDoesNotExist_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(new DeleteNoteCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
