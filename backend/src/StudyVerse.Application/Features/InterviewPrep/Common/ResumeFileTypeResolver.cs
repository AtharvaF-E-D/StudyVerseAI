using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.Common;

/// <summary>
/// Maps an uploaded resume file's name to the <see cref="NoteSourceFileType"/> that determines
/// which text-extraction strategy runs — reuses the exact same Phase 6
/// <c>ITextExtractionService</c> pipeline <c>UploadNoteCommandHandler</c> uses, restricted to
/// PDF/DOCX only (no image OCR for resumes, per the phase spec — see <c>NoteFileTypeResolver</c>
/// for the equivalent Notes-feature resolver this mirrors).
/// </summary>
public static class ResumeFileTypeResolver
{
    /// <summary>Upload size cap: 10 MB — same as <c>NoteFileTypeResolver</c>.</summary>
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public static bool TryResolve(string? fileName, out NoteSourceFileType fileType)
    {
        var extension = string.IsNullOrEmpty(fileName) ? string.Empty : Path.GetExtension(fileName).ToLowerInvariant();

        switch (extension)
        {
            case ".pdf":
                fileType = NoteSourceFileType.Pdf;
                return true;
            case ".docx":
                fileType = NoteSourceFileType.Docx;
                return true;
            default:
                fileType = default;
                return false;
        }
    }
}
