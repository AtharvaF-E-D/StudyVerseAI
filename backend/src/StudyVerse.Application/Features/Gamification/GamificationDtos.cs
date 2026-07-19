namespace StudyVerse.Application.Features.Gamification;

public sealed record BadgeDto(
    Guid Id,
    string Title,
    string Description,
    string Category,
    bool IsEarned,
    DateTime? EarnedAtUtc);

public sealed record GetBadgesResultDto(int EarnedCount, int TotalCount, IReadOnlyList<BadgeDto> Badges);

public sealed record MissionDto(
    Guid Id,
    string Title,
    string Description,
    int TargetCount,
    int CurrentCount,
    bool IsCompleted,
    DateTime? CompletedAtUtc,
    int XpReward,
    int CoinReward);

public sealed record GetMissionsResultDto(
    DateOnly WeekStartDateUtc,
    int CompletedCount,
    int TotalCount,
    IReadOnlyList<MissionDto> Missions);

public sealed record DailyRewardStatusDto(
    bool ClaimedToday,
    int DayNumber,
    int TodayCoins,
    int TodayXp,
    int TomorrowCoins,
    int TomorrowXp,
    string? ActiveSeasonalEventName,
    int ActiveSeasonalEventBonusCoins);

public sealed record ClaimDailyRewardResultDto(
    int DayNumber,
    int CoinsAwarded,
    int XpAwarded,
    int NewXpTotal,
    int NewCoinsTotal,
    string? SeasonalEventName,
    int SeasonalEventBonusCoins);

public sealed record SpinStatusDto(bool SpunToday, string? TodaysPrizeLabel);

public sealed record SpinResultDto(
    string PrizeLabel,
    int CoinsAwarded,
    int XpAwarded,
    int NewXpTotal,
    int NewCoinsTotal);

public sealed record GamificationSummaryDto(
    int Level,
    int Xp,
    int Coins,
    int CurrentStreakDays,
    int BadgesEarnedCount,
    int TotalBadgesCount,
    int MissionsCompletedThisWeek,
    int TotalMissionsThisWeek,
    DailyRewardStatusDto DailyRewardStatus,
    SpinStatusDto SpinStatus);
