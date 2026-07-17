using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Turns an uploaded note file into plain text, dispatching on <see cref="NoteSourceFileType"/>:
/// PdfPig for PDF, DocumentFormat.OpenXml for DOCX, and an OpenAI vision call (via
/// <see cref="INoteGenerationProvider.DescribeImageAsync"/>) for images — deliberately not a
/// separate OCR pipeline, see that interface's doc comment.
/// </summary>
public interface ITextExtractionService
{
    /// <param name="fileContent">A fresh, readable-from-the-start stream of the file's bytes.</param>
    /// <param name="fileType">Which extraction strategy to use.</param>
    /// <param name="mediaType">The file's MIME type; only consulted for <see cref="NoteSourceFileType.Image"/>.</param>
    Task<string> ExtractTextAsync(
        Stream fileContent,
        NoteSourceFileType fileType,
        string mediaType,
        CancellationToken cancellationToken = default);
}
