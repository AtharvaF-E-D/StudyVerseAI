using StudyVerse.Application.Features.Leaderboard;
using StudyVerse.Application.Features.Notifications;

namespace StudyVerse.Application.Features.Dashboard;

public sealed record DashboardDto(
    int Xp,
    int Level,
    int Coins,
    StreakDto Streak,
    IReadOnlyList<TodaysChallengeDto> TodaysChallenges,
    IReadOnlyList<DailyActivityDto> WeeklyActivity,
    DashboardLeaderboardDto Leaderboard,
    DashboardNotificationsDto Notifications);

public sealed record StreakDto(int CurrentDays, int LongestDays, bool StudiedToday);

public sealed record TodaysChallengeDto(
    Guid Id,
    string Title,
    string Description,
    int XpReward,
    int CoinReward,
    bool IsCompleted);

/// <summary>
/// XP earned on a single calendar date. Deliberately has no per-session "minutes studied" field:
/// there's no time-tracking feature built yet (the Study Planner, phase 9). Add it here once that
/// phase lands instead of fabricating a zero-filled placeholder now.
/// </summary>
public sealed record DailyActivityDto(DateOnly Date, int XpEarned);

public sealed record DashboardLeaderboardDto(int MyRank, IReadOnlyList<LeaderboardEntryDto> Top);

public sealed record DashboardNotificationsDto(int UnreadCount, IReadOnlyList<NotificationDto> Recent);
