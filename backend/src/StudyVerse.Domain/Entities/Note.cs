using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Entities;

/// <summary>
/// A single uploaded study document (PDF/DOCX) or image belonging to one user, plus the pipeline
/// state that turns it into AI-generated study material. <see cref="StorageKey"/> is an opaque
/// handle understood only by whatever <c>IFileStorageService</c> implementation is registered
/// (local disk today; see that interface's doc comment) — never a raw file path callers should
/// construct themselves. <see cref="ExtractedText"/> is the plain-text form of the source
/// (PdfPig/OpenXml for documents, an AI vision transcription for images) that
/// <see cref="Entities.NoteContent"/> was generated from. <see cref="ErrorMessage"/> is populated
/// only when <see cref="Status"/> is <see cref="NoteStatus.Failed"/>, so the client has something
/// concrete to show instead of a note stuck with no explanation.
/// </summary>
public class Note
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Derived from the source file name at upload time, and refined to the extracted
    /// text's first line when that's short enough to read as a title.</summary>
    public string Title { get; set; } = string.Empty;

    public string SourceFileName { get; set; } = string.Empty;

    public NoteSourceFileType SourceFileType { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string ExtractedText { get; set; } = string.Empty;

    public NoteStatus Status { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public User? User { get; set; }

    public NoteContent? Content { get; set; }
}
