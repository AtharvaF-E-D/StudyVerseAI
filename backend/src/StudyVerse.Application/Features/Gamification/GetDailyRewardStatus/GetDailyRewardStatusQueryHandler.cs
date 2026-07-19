using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Features.Gamification.GetDailyRewardStatus;

/// <summary>
/// Reports whether today's daily reward has already been claimed, the current cycle day number, and
/// a preview of today's/tomorrow's reward. Mirrors the exact same day-number derivation
/// <c>ClaimDailyRewardCommandHandler</c> uses, so the preview always matches what claiming right now
/// would actually award.
/// </summary>
public sealed class GetDailyRewardStatusQueryHandler : IRequestHandler<GetDailyRewardStatusQuery, Result<DailyRewardStatusDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetDailyRewardStatusQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<DailyRewardStatusDto>> Handle(GetDailyRewardStatusQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<DailyRewardStatusDto>("User not found.", ResultErrorType.NotFound);
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var lastClaim = await _db.DailyRewardClaims
            .Where(c => c.UserId == request.UserId)
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

        var result = new DailyRewardStatusDto(
            claimedToday,
            dayNumber,
            todayScheduleCoins + eventBonusCoins,
            todayXp,
            tomorrowScheduleCoins + eventBonusCoins,
            tomorrowXp,
            activeEvent?.Name,
            eventBonusCoins);

        return Result.Success(result);
    }
}
