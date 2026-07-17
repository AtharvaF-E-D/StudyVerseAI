using MediatR;
using Microsoft.Extensions.Logging;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Notes.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Notes.UploadNote;

/// <summary>
/// The AI Notes pipeline, run synchronously within one request (no background job queue for this
/// pass — see <see cref="NoteStatus"/>'s doc comment): save the file, create a <c>Processing</c>
/// Note row, extract its text, make one OpenAI call for all seven pieces of generated content,
/// parse the response, and persist it. The Note row is saved BEFORE the slow extraction/AI steps
/// so the client always gets a real note id back to poll/retry against even if something after
/// that point fails — which is why failures from that point on are recorded as
/// <see cref="NoteStatus.Failed"/> on the note (with <see cref="Note.ErrorMessage"/> set) rather
/// than returned as a failed <see cref="Result{T}"/>: the upload itself genuinely succeeded.
/// </summary>
public sealed class UploadNoteCommandHandler : IRequestHandler<UploadNoteCommand, Result<NoteSummaryDto>>
{
    /// <summary>A first line longer than this doesn't read like a title, so the file-name-derived
    /// title is kept instead.</summary>
    private const int MaxFirstLineTitleLength = 100;

    private readonly IAppDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ITextExtractionService _textExtraction;
    private readonly INoteGenerationProvider _noteGenerationProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UploadNoteCommandHandler> _logger;

    public UploadNoteCommandHandler(
        IAppDbContext db,
        IFileStorageService fileStorage,
        ITextExtractionService textExtraction,
        INoteGenerationProvider noteGenerationProvider,
        IDateTimeProvider dateTimeProvider,
        ILogger<UploadNoteCommandHandler> logger)
    {
        _db = db;
        _fileStorage = fileStorage;
        _textExtraction = textExtraction;
        _noteGenerationProvider = noteGenerationProvider;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<NoteSummaryDto>> Handle(UploadNoteCommand request, CancellationToken cancellationToken)
    {
        // Already checked by UploadNoteCommandValidator, re-checked here because it determines
        // which extraction strategy runs and isn't something we can proceed without.
        if (!NoteFileTypeResolver.TryResolve(request.FileName, out var fileType))
        {
            return Result.Failure<NoteSummaryDto>(
                "Unsupported file type. Only PDF, DOCX, JPG, and PNG files are supported.");
        }

        var now = _dateTimeProvider.UtcNow;
        var storageKey = await _fileStorage.SaveAsync(request.FileStream, request.FileName, cancellationToken);

        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = BuildTitleFromFileName(request.FileName),
            SourceFileName = request.FileName,
            SourceFileType = fileType,
            StorageKey = storageKey,
            ExtractedText = string.Empty,
            Status = NoteStatus.Processing,
            CreatedAtUtc = now,
        };
        _db.Notes.Add(note);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var mediaType = NoteFileTypeResolver.ResolveImageMediaType(request.FileName);

            await using var readStream = await _fileStorage.OpenReadAsync(storageKey, cancellationToken);
            var extractedText = await _textExtraction.ExtractTextAsync(readStream, fileType, mediaType, cancellationToken);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                throw new InvalidOperationException("No text could be extracted from the uploaded file.");
            }

            note.ExtractedText = extractedText;
            note.Title = TryGetFirstLineTitle(extractedText) ?? note.Title;

            var rawJson = await _noteGenerationProvider.GenerateNoteContentJsonAsync(extractedText, cancellationToken);

            var parseResult = NoteAiResponseMapper.Parse(rawJson);
            if (parseResult.IsFailure)
            {
                throw new InvalidOperationException(parseResult.Error);
            }

            var noteContent = NoteAiResponseMapper.ToEntity(note.Id, parseResult.Value);
            _db.NoteContents.Add(noteContent);
            note.Status = NoteStatus.Ready;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI note content for note {NoteId}", note.Id);
            note.Status = NoteStatus.Failed;
            note.ErrorMessage = Truncate(ex.Message, 2000);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new NoteSummaryDto(note.Id, note.Title, note.Status, note.CreatedAtUtc));
    }

    private static string BuildTitleFromFileName(string fileName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var spaced = nameWithoutExtension.Replace('_', ' ').Replace('-', ' ').Trim();
        return string.IsNullOrWhiteSpace(spaced) ? "Untitled note" : spaced;
    }

    private static string? TryGetFirstLineTitle(string extractedText)
    {
        var firstLine = extractedText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(firstLine) || firstLine.Length > MaxFirstLineTitleLength
            ? null
            : firstLine;
    }

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength];
}
