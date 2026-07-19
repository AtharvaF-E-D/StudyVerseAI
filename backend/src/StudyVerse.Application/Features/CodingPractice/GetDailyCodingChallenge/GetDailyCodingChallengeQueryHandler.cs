using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.CodingPractice;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.GetDailyCodingChallenge;

/// <summary>
/// Orders the full seeded problem pool by <c>Id</c> (fixed hardcoded GUIDs - see
/// <c>CodingProblemSeedData</c> - so this ordering never changes across migrations) and hands that
/// list to the pure <see cref="DailyCodingChallengeSelector"/> to pick today's UTC-day rotation.
/// </summary>
public sealed class GetDailyCodingChallengeQueryHandler : IRequestHandler<GetDailyCodingChallengeQuery, Result<DailyCodingChallengeDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetDailyCodingChallengeQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<DailyCodingChallengeDto>> Handle(GetDailyCodingChallengeQuery request, CancellationToken cancellationToken)
    {
        var orderedIds = await _db.CodingProblems
            .OrderBy(p => p.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (orderedIds.Count == 0)
        {
            return Result.Failure<DailyCodingChallengeDto>("No coding problems are available.", ResultErrorType.NotFound);
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var problemId = DailyCodingChallengeSelector.GetTodaysProblemId(orderedIds, today);

        var problem = await _db.CodingProblems.FirstAsync(p => p.Id == problemId, cancellationToken);

        return Result.Success(new DailyCodingChallengeDto(problem.Id, problem.Title, problem.Difficulty));
    }
}
