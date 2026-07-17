using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Notes.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notes.GetNotes;

public sealed class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, Result<IReadOnlyList<NoteSummaryDto>>>
{
    private readonly IAppDbContext _db;

    public GetNotesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<NoteSummaryDto>>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
    {
        var notes = await _db.Notes
            .Where(n => n.UserId == request.UserId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(request.Take)
            .Select(n => new NoteSummaryDto(n.Id, n.Title, n.Status, n.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<NoteSummaryDto>>(notes);
    }
}
