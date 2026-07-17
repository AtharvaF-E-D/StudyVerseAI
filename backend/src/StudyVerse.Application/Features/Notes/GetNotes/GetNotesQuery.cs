using MediatR;
using StudyVerse.Application.Features.Notes.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notes.GetNotes;

public sealed record GetNotesQuery(Guid UserId, int Take) : IRequest<Result<IReadOnlyList<NoteSummaryDto>>>;
