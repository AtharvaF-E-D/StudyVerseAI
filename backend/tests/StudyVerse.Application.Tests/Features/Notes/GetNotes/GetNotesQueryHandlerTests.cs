using FluentAssertions;
using StudyVerse.Application.Features.Notes.GetNotes;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.Notes.GetNotes;

public sealed class GetNotesQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();

    private GetNotesQueryHandler CreateHandler() => new(_db);

    [Fact]
    public async Task Handle_ReturnsOnlyTheRequestingUsersNotes_NewestFirst()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _db.Notes.AddRange(
            new Note
            {
                Id = Guid.NewGuid(), UserId = userId, Title = "Older note", SourceFileName = "a.pdf",
                SourceFileType = NoteSourceFileType.Pdf, StorageKey = "a", ExtractedText = "x",
                Status = NoteStatus.Ready, CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new Note
            {
                Id = Guid.NewGuid(), UserId = userId, Title = "Newer note", SourceFileName = "b.pdf",
                SourceFileType = NoteSourceFileType.Pdf, StorageKey = "b", ExtractedText = "x",
                Status = NoteStatus.Ready, CreatedAtUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            },
            new Note
            {
                Id = Guid.NewGuid(), UserId = otherUserId, Title = "Someone else's note", SourceFileName = "c.pdf",
                SourceFileType = NoteSourceFileType.Pdf, StorageKey = "c", ExtractedText = "x",
                Status = NoteStatus.Ready, CreatedAtUtc = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc),
            });
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetNotesQuery(userId, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(n => n.Title).Should().ContainInOrder("Newer note", "Older note");
    }

    [Fact]
    public async Task Handle_RespectsTakeLimit()
    {
        var userId = Guid.NewGuid();
        for (var i = 0; i < 5; i++)
        {
            _db.Notes.Add(new Note
            {
                Id = Guid.NewGuid(), UserId = userId, Title = $"Note {i}", SourceFileName = "n.pdf",
                SourceFileType = NoteSourceFileType.Pdf, StorageKey = $"key-{i}", ExtractedText = "x",
                Status = NoteStatus.Ready, CreatedAtUtc = DateTime.UtcNow.AddMinutes(i),
            });
        }
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetNotesQuery(userId, 2), CancellationToken.None);

        result.Value.Should().HaveCount(2);
    }
}
