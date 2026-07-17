using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notes.DeleteNote;

public sealed record DeleteNoteCommand(Guid UserId, Guid NoteId) : IRequest<Result>;
