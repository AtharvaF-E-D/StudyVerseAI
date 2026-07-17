using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notes.DeleteNote;

/// <summary>
/// Deletes both the database rows (Note + its 1:1 NoteContent, via DB cascade — see
/// <c>NoteConfiguration</c>) and the underlying stored file. The DB delete happens first: if the
/// storage delete then fails, the user-visible outcome (the note is gone from their list) still
/// happened correctly, and we merely leak an orphaned file on disk — logged as a warning rather
/// than failing a request whose primary effect already succeeded.
/// </summary>
public sealed class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<DeleteNoteCommandHandler> _logger;

    public DeleteNoteCommandHandler(IAppDbContext db, IFileStorageService fileStorage, ILogger<DeleteNoteCommandHandler> logger)
    {
        _db = db;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _db.Notes.FirstOrDefaultAsync(
            n => n.Id == request.NoteId && n.UserId == request.UserId,
            cancellationToken);

        if (note is null)
        {
            return Result.Failure("Note not found.", ResultErrorType.NotFound);
        }

        var storageKey = note.StorageKey;

        _db.Notes.Remove(note);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await _fileStorage.DeleteAsync(storageKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete stored file for note {NoteId} (storage key {StorageKey})", request.NoteId, storageKey);
        }

        return Result.Success();
    }
}
