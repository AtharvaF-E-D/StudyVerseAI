using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.GetSubmissions;

public sealed class GetSubmissionsQueryHandler : IRequestHandler<GetSubmissionsQuery, Result<IReadOnlyList<CodeSubmissionDto>>>
{
    private readonly IAppDbContext _db;

    public GetSubmissionsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<CodeSubmissionDto>>> Handle(GetSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.CodeSubmissions.Where(s => s.UserId == request.UserId);

        if (request.ProblemId is { } problemId)
        {
            query = query.Where(s => s.ProblemId == problemId);
        }

        // Join first (keeping the two source entities separate, not yet projected into the DTO
        // record) and order by the raw entity property, THEN project into CodeSubmissionDto as the
        // very last step. Ordering by a property read back off an already-constructed DTO record
        // (as a prior version of this query did) fails to translate against the real Npgsql
        // provider ("could not be translated") even though EF Core's InMemory test provider
        // tolerates it - this shape is the one that generates valid SQL against both.
        var submissions = await query
            .Join(_db.CodingProblems, s => s.ProblemId, p => p.Id, (s, p) => new { Submission = s, p.Title })
            .OrderByDescending(x => x.Submission.SubmittedAtUtc)
            .Select(x => new CodeSubmissionDto(
                x.Submission.Id,
                x.Submission.ProblemId,
                x.Title,
                x.Submission.LanguageId,
                x.Submission.Status,
                x.Submission.TestsPassed,
                x.Submission.TotalTests,
                x.Submission.SubmittedAtUtc))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<CodeSubmissionDto>>(submissions);
    }
}
