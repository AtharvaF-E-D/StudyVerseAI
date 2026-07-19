using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.CodingPractice;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.CodingPractice.GetCodingStats;

/// <summary>
/// <see cref="CodingStatsDto.CurrentDailyStreak"/> is computed on read (not a persisted counter like
/// <c>UserProgress.CurrentStreakDays</c>, which tracks general daily activity, not specifically
/// daily-challenge solves) by walking backwards day-by-day from today and checking, for each day,
/// whether that day's <c>DailyCodingChallengeSelector</c>-rotated problem has an
/// <see cref="CodeSubmissionStatus.Accepted"/> submission dated that same UTC calendar day. If
/// today's own daily challenge hasn't been solved yet, the walk starts from yesterday instead - a
/// still-pending "today" doesn't break a streak that's otherwise unbroken through yesterday
/// (matching typical daily-streak UX, e.g. the user has until the day ends to keep it alive).
/// </summary>
public sealed class GetCodingStatsQueryHandler : IRequestHandler<GetCodingStatsQuery, Result<CodingStatsDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetCodingStatsQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<CodingStatsDto>> Handle(GetCodingStatsQuery request, CancellationToken cancellationToken)
    {
        var solvedDifficulties = await _db.CodeSubmissions
            .Where(s => s.UserId == request.UserId && s.Status == CodeSubmissionStatus.Accepted)
            .Select(s => s.ProblemId)
            .Distinct()
            .Join(_db.CodingProblems, id => id, p => p.Id, (id, p) => p.Difficulty)
            .ToListAsync(cancellationToken);

        var solvedByDifficulty = new SolvedByDifficultyDto(
            solvedDifficulties.Count(d => d == CodingDifficulty.Easy),
            solvedDifficulties.Count(d => d == CodingDifficulty.Medium),
            solvedDifficulties.Count(d => d == CodingDifficulty.Hard));

        var totalSubmissions = await _db.CodeSubmissions.CountAsync(s => s.UserId == request.UserId, cancellationToken);

        var currentDailyStreak = await ComputeDailyStreakAsync(request.UserId, cancellationToken);

        var dto = new CodingStatsDto(solvedDifficulties.Count, solvedByDifficulty, totalSubmissions, currentDailyStreak);
        return Result.Success(dto);
    }

    private async Task<int> ComputeDailyStreakAsync(Guid userId, CancellationToken cancellationToken)
    {
        var orderedIds = await _db.CodingProblems.OrderBy(p => p.Id).Select(p => p.Id).ToListAsync(cancellationToken);
        if (orderedIds.Count == 0)
        {
            return 0;
        }

        var acceptedDatesByProblem = await _db.CodeSubmissions
            .Where(s => s.UserId == userId && s.Status == CodeSubmissionStatus.Accepted)
            .Select(s => new { s.ProblemId, s.SubmittedAtUtc })
            .ToListAsync(cancellationToken);

        var solvedOn = acceptedDatesByProblem
            .Select(s => (s.ProblemId, Date: DateOnly.FromDateTime(s.SubmittedAtUtc)))
            .ToHashSet();

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        bool SolvedDailyChallengeOn(DateOnly day)
        {
            var dailyProblemId = DailyCodingChallengeSelector.GetTodaysProblemId(orderedIds, day);
            return solvedOn.Contains((dailyProblemId, day));
        }

        var cursor = today;
        if (!SolvedDailyChallengeOn(cursor))
        {
            cursor = cursor.AddDays(-1);
        }

        var streak = 0;
        while (SolvedDailyChallengeOn(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }
}
