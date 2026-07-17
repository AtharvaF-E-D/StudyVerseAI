using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Enums;
using UglyToad.PdfPig;

namespace StudyVerse.Infrastructure.Files;

/// <summary>
/// Extracts plain text from an uploaded note file. PDF and DOCX are parsed for real (PdfPig,
/// DocumentFormat.OpenXml — no external processes or native dependencies); images have no
/// separate OCR pipeline and instead delegate to <see cref="INoteGenerationProvider.DescribeImageAsync"/>,
/// reusing the existing OpenAI vision integration instead of adding a new dependency.
/// </summary>
public sealed class DocumentTextExtractionService : ITextExtractionService
{
    private readonly INoteGenerationProvider _noteGenerationProvider;

    public DocumentTextExtractionService(INoteGenerationProvider noteGenerationProvider)
    {
        _noteGenerationProvider = noteGenerationProvider;
    }

    public async Task<string> ExtractTextAsync(
        Stream fileContent,
        NoteSourceFileType fileType,
        string mediaType,
        CancellationToken cancellationToken = default)
    {
        return fileType switch
        {
            NoteSourceFileType.Pdf => ExtractPdfText(fileContent),
            NoteSourceFileType.Docx => ExtractDocxText(fileContent),
            NoteSourceFileType.Image => await _noteGenerationProvider.DescribeImageAsync(fileContent, mediaType, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, "Unsupported note source file type."),
        };
    }

    /// <summary>Extracts each page's plain text via PdfPig and joins pages with a blank line, so
    /// the resulting text still reads as one continuous document while preserving page breaks.</summary>
    private static string ExtractPdfText(Stream fileContent)
    {
        using var document = PdfDocument.Open(fileContent);
        var pageTexts = document.GetPages().Select(page => page.Text);
        return string.Join("\n\n", pageTexts);
    }

    /// <summary>Extracts the body's paragraph text via DocumentFormat.OpenXml. Tables/headers/
    /// footers are intentionally out of scope for this pass — body paragraphs cover the vast
    /// majority of academic notes content.</summary>
    private static string ExtractDocxText(Stream fileContent)
    {
        using var wordDocument = WordprocessingDocument.Open(fileContent, false);
        var body = wordDocument.MainDocumentPart?.Document?.Body;
        if (body is null)
        {
            return string.Empty;
        }

        var paragraphTexts = body.Descendants<Paragraph>().Select(paragraph => paragraph.InnerText);
        return string.Join("\n", paragraphTexts);
    }
}
