using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Gamification.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Features.Gamification.GetSummary;

/// <summary>
/// One call for a gamification hub screen: level/xp/coins/streak (already tracked since Phase 3),
/// plus badge/mission/daily-reward/spin status - reuses the same
/// <see cref="BadgeEvaluationService"/>/<see cref="MissionProgressService"/> shared builders
/// <c>GetBadgesQueryHandler</c>/<c>GetMissionsQueryHandler</c> use, and the same day-number
/// derivation <c>GetDailyRewardStatusQueryHandler</c>/<c>GetSpinStatusQueryHandler</c> use, so all
/// four endpoints and this summary always agree.
/// </summary>
public sealed class GetGamificationSummaryQueryHandler : IRequestHandler<GetGamificationSummaryQuery, Result<GamificationSummaryDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly BadgeEvaluationService _badgeEvaluationService;
    private readonly MissionProgressService _missionProgressService;

    public GetGamificationSummaryQueryHandler(
        IAppDbContext db,
        IDateTimeProvider dateTimeProvider,
        BadgeEvaluationService badgeEvaluationService,
        MissionProgressService missionProgressService)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _badgeEvaluationService = badgeEvaluationService;
        _missionProgressService = missionProgressService;
    }

    public async Task<Result<GamificationSummaryDto>> Handle(GetGamificationSummaryQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<GamificationSummaryDto>("User not found.", ResultErrorType.NotFound);
        }

        var progress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        var xp = progress?.Xp ?? 0;
        var coins = progress?.Coins ?? 0;
        var currentStreakDays = progress?.CurrentStreakDays ?? 0;

        await _badgeEvaluationService.EvaluateAsync(request.UserId, cancellationToken);
        var badgesEarnedCount = await _db.UserBadges.CountAsync(b => b.UserId == request.UserId, cancellationToken);

        var missionResults = await _missionProgressService.RefreshThisWeeksMissionsAsync(request.UserId, cancellationToken);

        var now = _dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var dailyRewardStatus = await BuildDailyRewardStatusAsync(request.UserId, today, cancellationToken);
        var spinStatus = await BuildSpinStatusAsync(request.UserId, today, cancellationToken);

        // Re-read UserProgress in case the badge/mission evaluation above just credited a mission reward.
        progress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        xp = progress?.Xp ?? xp;
        coins = progress?.Coins ?? coins;

        var summary = new GamificationSummaryDto(
            LevelCalculator.GetLevel(xp),
            xp,
            coins,
            currentStreakDays,
            badgesEarnedCount,
            BadgeCatalog.All.Count,
            missionResults.Count(r => r.IsCompleted),
            missionResults.Count,
            dailyRewardStatus,
            spinStatus);

        return Result.Success(summary);
    }

    private async Task<DailyRewardStatusDto> BuildDailyRewardStatusAsync(Guid userId, DateOnly today, CancellationToken cancellationToken)
    {
        var lastClaim = await _db.DailyRewardClaims
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ClaimDateUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var claimedToday = lastClaim is not null && lastClaim.ClaimDateUtc == today;

        int dayNumber;
        if (claimedToday)
        {
            dayNumber = lastClaim!.ConsecutiveDayNumber;
        }
        else if (lastClaim is not null && lastClaim.ClaimDateUtc == today.AddDays(-1))
        {
            dayNumber = DailyRewardSchedule.NextDayNumber(lastClaim.ConsecutiveDayNumber);
        }
        else
        {
            dayNumber = 1;
        }

        var (todayScheduleCoins, todayXp) = DailyRewardSchedule.GetReward(dayNumber);
        var tomorrowDayNumber = DailyRewardSchedule.NextDayNumber(dayNumber);
        var (tomorrowScheduleCoins, tomorrowXp) = DailyRewardSchedule.GetReward(tomorrowDayNumber);

        var activeEvent = SeasonalEventCatalog.GetActiveEvent(today);
        var eventBonusCoins = activeEvent?.DailyRewardBonusCoins ?? 0;

        return new DailyRewardStatusDto(
            claimedToday,
            dayNumber,
            todayScheduleCoins + eventBonusCoins,
            todayXp,
            tomorrowScheduleCoins + eventBonusCoins,
            tomorrowXp,
            activeEvent?.Name,
            eventBonusCoins);
    }

    private async Task<SpinStatusDto> BuildSpinStatusAsync(Guid userId, DateOnly today, CancellationToken cancellationToken)
    {
        var todaysSpin = await _db.SpinResults
            .Where(s => s.UserId == userId && s.SpinDateUtc == today)
            .Select(s => s.PrizeLabel)
            .FirstOrDefaultAsync(cancellationToken);

        return new SpinStatusDto(todaysSpin is not null, todaysSpin);
    }
}
