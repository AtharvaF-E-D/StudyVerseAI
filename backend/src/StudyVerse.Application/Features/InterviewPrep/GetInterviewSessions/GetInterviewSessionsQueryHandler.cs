using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewSessions;

public sealed class GetInterviewSessionsQueryHandler
    : IRequestHandler<GetInterviewSessionsQuery, Result<IReadOnlyList<InterviewSessionSummaryDto>>>
{
    private readonly IAppDbContext _db;

    public GetInterviewSessionsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<InterviewSessionSummaryDto>>> Handle(
        GetInterviewSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var sessions = await _db.InterviewSessions
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s => s.StartedAtUtc)
            .Select(s => new InterviewSessionSummaryDto(s.Id, s.Type, s.Status, s.OverallScore, s.StartedAtUtc, s.CompletedAtUtc))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<InterviewSessionSummaryDto>>(sessions);
    }
}
