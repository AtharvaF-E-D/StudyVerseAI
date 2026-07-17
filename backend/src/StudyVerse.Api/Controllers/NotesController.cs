using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Api.Contracts;
using StudyVerse.Application.Features.Notes.DeleteNote;
using StudyVerse.Application.Features.Notes.GetNote;
using StudyVerse.Application.Features.Notes.GetNotes;
using StudyVerse.Application.Features.Notes.UploadNote;

namespace StudyVerse.Api.Controllers;

/// <summary>
/// AI Notes: upload a PDF/DOCX/image, get back AI-generated summary, key points, flashcards,
/// mcqs, mind map outline, revision sheet, vocabulary, and formulas. Upload is synchronous — the
/// response only comes back once the note reaches <c>Ready</c> or <c>Failed</c> (no background job
/// queue/polling for this pass, see <see cref="StudyVerse.Domain.Enums.NoteStatus"/>'s doc comment).
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notes")]
[Authorize]
public sealed class NotesController : ApiControllerBase
{
    // A little above UploadNoteCommandValidator's 10MB cap so oversized uploads still reach the
    // handler and get FluentValidation's friendly error message, rather than a raw 413 from Kestrel.
    private const long MaxRequestBodyBytes = 12 * 1024 * 1024;

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBodyBytes)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new ApiErrorResponse("A file is required."));
        }

        await using var stream = file.OpenReadStream();
        var command = new UploadNoteCommand(userId, stream, file.FileName, file.ContentType, file.Length);

        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotes([FromQuery] int take, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var effectiveTake = take <= 0 ? 20 : take;

        var result = await Mediator.Send(new GetNotesQuery(userId, effectiveTake), cancellationToken);

        return FromResult(result, notes => Ok(notes));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNote(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetNoteQuery(userId, id), cancellationToken);

        return FromResult(result, note => Ok(note));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNote(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new DeleteNoteCommand(userId, id), cancellationToken);

        return FromResult(result, () => NoContent());
    }
}
