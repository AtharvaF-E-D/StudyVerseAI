using MediatR;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Leaderboard.GetLeaderboard;

/// <summary>
/// Returns the top N leaderboard entries by Xp descending. The caller's own entry is appended at
/// the end if it falls outside the requested Take, so a caller always knows their own standing —
/// consistent with how <c>DashboardDto.Leaderboard.MyRank</c> always surfaces the caller's rank.
/// </summary>
public sealed class GetLeaderboardQueryHandler
    : IRequestHandler<GetLeaderboardQuery, Result<IReadOnlyList<LeaderboardEntryDto>>>
{
    private readonly IAppDbContext _db;

    public GetLeaderboardQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<LeaderboardEntryDto>>> Handle(
        GetLeaderboardQuery request,
        CancellationToken cancellationToken)
    {
        var ranked = await LeaderboardBuilder.GetRankedEntriesAsync(_db, cancellationToken);

        var top = ranked.Take(request.Take).ToList();

        if (top.All(e => e.UserId != request.UserId))
        {
            var mine = ranked.FirstOrDefault(e => e.UserId == request.UserId);
            if (mine is not null)
            {
                top.Add(mine);
            }
        }

        return Result.Success<IReadOnlyList<LeaderboardEntryDto>>(top);
    }
}
