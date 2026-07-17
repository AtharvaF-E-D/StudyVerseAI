using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Notes.Common;

/// <summary>
/// Maps an uploaded file's name to the <see cref="NoteSourceFileType"/> that determines both
/// upload validation and which text-extraction strategy runs. Deliberately keyed off the file
/// extension rather than the client-supplied Content-Type header — browsers/mobile clients aren't
/// always consistent about setting it (some send "application/octet-stream" for anything), and the
/// extension is what the user actually picked.
/// </summary>
public static class NoteFileTypeResolver
{
    /// <summary>Upload size cap: 10 MB.</summary>
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
            case ".jpg":
            case ".jpeg":
            case ".png":
                fileType = NoteSourceFileType.Image;
                return true;
            default:
                fileType = default;
                return false;
        }
    }

    /// <summary>The MIME type to send an image file to OpenAI's vision API as. Only meaningful when
    /// <see cref="TryResolve"/> returned <see cref="NoteSourceFileType.Image"/>.</summary>
    public static string ResolveImageMediaType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream",
        };
}
