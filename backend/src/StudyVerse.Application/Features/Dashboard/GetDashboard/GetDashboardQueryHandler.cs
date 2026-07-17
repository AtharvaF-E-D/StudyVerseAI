using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Leaderboard;
using StudyVerse.Application.Features.Notifications;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Features.Dashboard.GetDashboard;

public sealed class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private const int WeeklyActivityDays = 7;
    private const int LeaderboardTopCount = 5;
    private const int RecentNotificationsCount = 5;

    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetDashboardQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<DashboardDto>("User not found.", ResultErrorType.NotFound);
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var progress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        var xp = progress?.Xp ?? 0;
        var coins = progress?.Coins ?? 0;
        var streak = new StreakDto(
            CurrentDays: progress?.CurrentStreakDays ?? 0,
            LongestDays: progress?.LongestStreakDays ?? 0,
            StudiedToday: progress?.LastActivityDateUtc == today);

        var todaysChallenges = await BuildTodaysChallengesAsync(request.UserId, today, cancellationToken);
        var weeklyActivity = await BuildWeeklyActivityAsync(request.UserId, today, cancellationToken);

        var ranked = await LeaderboardBuilder.GetRankedEntriesAsync(_db, cancellationToken);
        var myRank = ranked.FirstOrDefault(e => e.UserId == request.UserId)?.Rank ?? 0;
        var leaderboard = new DashboardLeaderboardDto(myRank, ranked.Take(LeaderboardTopCount).ToList());

        var notifications = await BuildNotificationsAsync(request.UserId, cancellationToken);

        var dashboard = new DashboardDto(
            xp,
            LevelCalculator.GetLevel(xp),
            coins,
            streak,
            todaysChallenges,
            weeklyActivity,
            leaderboard,
            notifications);

        return Result.Success(dashboard);
    }

    private async Task<IReadOnlyList<TodaysChallengeDto>> BuildTodaysChallengesAsync(
        Guid userId,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var todaysTemplates = DailyChallengeSelector.GetTodaysTemplates(today);
        var templateIds = todaysTemplates.Select(t => t.Id).ToList();

        var completedTemplateIds = await _db.ChallengeCompletions
            .Where(c => c.UserId == userId && c.CompletedDateUtc == today && templateIds.Contains(c.ChallengeTemplateId))
            .Select(c => c.ChallengeTemplateId)
            .ToListAsync(cancellationToken);

        return todaysTemplates
            .Select(t => new TodaysChallengeDto(
                t.Id,
                t.Title,
                t.Description,
                t.XpReward,
                t.CoinReward,
                completedTemplateIds.Contains(t.Id)))
            .ToList();
    }

    private async Task<IReadOnlyList<DailyActivityDto>> BuildWeeklyActivityAsync(
        Guid userId,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var weekStart = today.AddDays(-(WeeklyActivityDays - 1));

        var completions = await _db.ChallengeCompletions
            .Where(c => c.UserId == userId && c.CompletedDateUtc >= weekStart && c.CompletedDateUtc <= today)
            .GroupBy(c => c.CompletedDateUtc)
            .Select(g => new { Date = g.Key, XpEarned = g.Sum(c => c.XpAwarded) })
            .ToListAsync(cancellationToken);

        var xpByDate = completions.ToDictionary(c => c.Date, c => c.XpEarned);

        return Enumerable.Range(0, WeeklyActivityDays)
            .Select(offset =>
            {
                var date = weekStart.AddDays(offset);
                // NOTE: no per-session minutesStudied here yet — add real time-tracking data to
                // this DTO once the Study Planner (Phase 9) lands.
                return new DailyActivityDto(date, xpByDate.GetValueOrDefault(date, 0));
            })
            .ToList();
    }

    private async Task<DashboardNotificationsDto> BuildNotificationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var unreadCount = await _db.Notifications
            .CountAsync(n => n.UserId == userId && n.ReadAtUtc == null, cancellationToken);

        var recent = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(RecentNotificationsCount)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Body, n.CreatedAtUtc, n.ReadAtUtc))
            .ToListAsync(cancellationToken);

        return new DashboardNotificationsDto(unreadCount, recent);
    }
}
