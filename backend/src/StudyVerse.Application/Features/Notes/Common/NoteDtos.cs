using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Notes.Common;

/// <summary>Shape returned by <c>GetNotesQuery</c> (the list view) and by <c>UploadNoteCommand</c>
/// once processing finishes.</summary>
public sealed record NoteSummaryDto(Guid Id, string Title, NoteStatus Status, DateTime CreatedAtUtc);

/// <summary>Shape returned by <c>GetNoteQuery</c>. <see cref="Content"/> is null until/unless
/// <see cref="Status"/> is <see cref="NoteStatus.Ready"/>; <see cref="ErrorMessage"/> is populated
/// only when <see cref="Status"/> is <see cref="NoteStatus.Failed"/>.</summary>
public sealed record NoteDetailDto(
    Guid Id,
    string Title,
    string SourceFileName,
    NoteSourceFileType SourceFileType,
    NoteStatus Status,
    string ExtractedText,
    string? ErrorMessage,
    DateTime CreatedAtUtc,
    NoteContentDto? Content);
