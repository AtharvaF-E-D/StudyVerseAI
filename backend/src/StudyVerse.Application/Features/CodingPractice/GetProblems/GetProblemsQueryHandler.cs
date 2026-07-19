using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.CodingPractice.GetProblems;

/// <summary>
/// Problems are global (not owned by a user), so filtering is purely by difficulty/category/
/// interview-only; <see cref="CodingProblemSummaryDto.IsSolved"/> is the only per-user field, worked
/// out from a single distinct-<c>ProblemId</c> query against this user's <c>Accepted</c> submissions
/// rather than N+1 lookups per problem.
/// </summary>
public sealed class GetProblemsQueryHandler : IRequestHandler<GetProblemsQuery, Result<IReadOnlyList<CodingProblemSummaryDto>>>
{
    private readonly IAppDbContext _db;

    public GetProblemsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<CodingProblemSummaryDto>>> Handle(GetProblemsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.CodingProblems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Difficulty) &&
            Enum.TryParse<CodingDifficulty>(request.Difficulty, ignoreCase: true, out var difficulty))
        {
            query = query.Where(p => p.Difficulty == difficulty);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var normalizedCategory = request.Category.Trim();
            query = query.Where(p => p.Category.ToLower() == normalizedCategory.ToLower());
        }

        if (request.InterviewOnly == true)
        {
            query = query.Where(p => p.IsInterviewQuestion);
        }

        var problems = await query
            .OrderBy(p => p.Difficulty)
            .ThenBy(p => p.Title)
            .ToListAsync(cancellationToken);

        var solvedProblemIds = await _db.CodeSubmissions
            .Where(s => s.UserId == request.UserId && s.Status == CodeSubmissionStatus.Accepted)
            .Select(s => s.ProblemId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var solvedSet = solvedProblemIds.ToHashSet();

        var dtos = problems
            .Select(p => new CodingProblemSummaryDto(
                p.Id, p.Title, p.Difficulty, p.Category, p.IsInterviewQuestion, solvedSet.Contains(p.Id)))
            .ToList();

        return Result.Success<IReadOnlyList<CodingProblemSummaryDto>>(dtos);
    }
}
