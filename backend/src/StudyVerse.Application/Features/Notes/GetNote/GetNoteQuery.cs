using MediatR;
using StudyVerse.Application.Features.Notes.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notes.GetNote;

public sealed record GetNoteQuery(Guid UserId, Guid NoteId) : IRequest<Result<NoteDetailDto>>;
