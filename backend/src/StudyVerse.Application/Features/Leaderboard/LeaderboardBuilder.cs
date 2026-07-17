using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;

namespace StudyVerse.Application.Features.Leaderboard;

/// <summary>
/// Shared ranking logic used by both the standalone leaderboard endpoint and the dashboard's
/// embedded leaderboard summary.
///
/// Rank is computed in-memory (RANK() semantics: tied Xp values share a rank, and the next
/// distinct value skips accordingly) after materializing users and progress rows, rather than via
/// a raw-SQL window function. This keeps <see cref="IAppDbContext"/> free of any provider-specific
/// SQL and is simple/cheap enough given the expected user-base size at this phase; revisit with a
/// window-function query (or a materialized leaderboard cache) if the user table grows large.
/// </summary>
internal static class LeaderboardBuilder
{
    public static async Task<IReadOnlyList<LeaderboardEntryDto>> GetRankedEntriesAsync(
        IAppDbContext db,
        CancellationToken cancellationToken)
    {
        var users = await db.Users
            .Select(u => new { u.Id, u.DisplayName })
            .ToListAsync(cancellationToken);

        var xpByUserId = await db.UserProgresses
            .Select(p => new { p.UserId, p.Xp })
            .ToDictionaryAsync(p => p.UserId, p => p.Xp, cancellationToken);

        var ordered = users
            .Select(u => new { u.Id, u.DisplayName, Xp = xpByUserId.GetValueOrDefault(u.Id, 0) })
            .OrderByDescending(u => u.Xp)
            .ThenBy(u => u.Id)
            .ToList();

        var entries = new List<LeaderboardEntryDto>(ordered.Count);
        var rank = 0;
        int? previousXp = null;

        for (var i = 0; i < ordered.Count; i++)
        {
            var row = ordered[i];
            if (previousXp is null || row.Xp != previousXp)
            {
                rank = i + 1;
            }

            entries.Add(new LeaderboardEntryDto(row.Id, row.DisplayName, row.Xp, rank));
            previousXp = row.Xp;
        }

        return entries;
    }
}
