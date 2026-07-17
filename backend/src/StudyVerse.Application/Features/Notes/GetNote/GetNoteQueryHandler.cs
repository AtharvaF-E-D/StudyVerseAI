using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Notes.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notes.GetNote;

public sealed class GetNoteQueryHandler : IRequestHandler<GetNoteQuery, Result<NoteDetailDto>>
{
    private readonly IAppDbContext _db;

    public GetNoteQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<NoteDetailDto>> Handle(GetNoteQuery request, CancellationToken cancellationToken)
    {
        var note = await _db.Notes
            .Include(n => n.Content)
            .FirstOrDefaultAsync(n => n.Id == request.NoteId && n.UserId == request.UserId, cancellationToken);

        if (note is null)
        {
            return Result.Failure<NoteDetailDto>("Note not found.", ResultErrorType.NotFound);
        }

        var contentDto = note.Content is not null ? NoteAiResponseMapper.FromEntity(note.Content) : null;

        var dto = new NoteDetailDto(
            note.Id,
            note.Title,
            note.SourceFileName,
            note.SourceFileType,
            note.Status,
            note.ExtractedText,
            note.ErrorMessage,
            note.CreatedAtUtc,
            contentDto);

        return Result.Success(dto);
    }
}
