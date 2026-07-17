using MediatR;
using StudyVerse.Application.Features.Notes.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notes.UploadNote;

/// <param name="FileStream">A fresh, readable-from-the-start stream of the uploaded file's bytes
/// (e.g. <c>IFormFile.OpenReadStream()</c> — the Application layer never references ASP.NET Core's
/// <c>IFormFile</c> directly).</param>
/// <param name="FileSizeBytes">The file's declared size, checked by the validator without touching
/// <paramref name="FileStream"/> itself.</param>
public sealed record UploadNoteCommand(Guid UserId, Stream FileStream, string FileName, string ContentType, long FileSizeBytes)
    : IRequest<Result<NoteSummaryDto>>;
