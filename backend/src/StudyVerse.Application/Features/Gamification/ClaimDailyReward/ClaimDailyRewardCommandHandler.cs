using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Features.Gamification.ClaimDailyReward;

/// <summary>
/// Claims today's daily-login reward. Rejects a second claim on the same UTC calendar date; the day
/// number resets to 1 on any gap since the last claim (not just "yesterday exactly"), the same
/// date-rollover reasoning <see cref="StudyVerse.Application.Common.Services.StreakService"/> uses.
/// </summary>
public sealed class ClaimDailyRewardCommandHandler : IRequestHandler<ClaimDailyRewardCommand, Result<ClaimDailyRewardResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ClaimDailyRewardCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<ClaimDailyRewardResultDto>> Handle(ClaimDailyRewardCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<ClaimDailyRewardResultDto>("User not found.", ResultErrorType.NotFound);
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var lastClaim = await _db.DailyRewardClaims
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.ClaimDateUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastClaim is not null && lastClaim.ClaimDateUtc == today)
        {
            return Result.Failure<ClaimDailyRewardResultDto>(
                "You've already claimed today's daily reward. Come back tomorrow!",
                ResultErrorType.Conflict);
        }

        var dayNumber = lastClaim is not null && lastClaim.ClaimDateUtc == today.AddDays(-1)
            ? DailyRewardSchedule.NextDayNumber(lastClaim.ConsecutiveDayNumber)
            : 1;

        var (scheduleCoins, xp) = DailyRewardSchedule.GetReward(dayNumber);

        // The one seasonal-event hook this pass implements: a flat coin bonus on top of the
        // regular schedule while a SeasonalEventCatalog event is running - see that catalog's doc
        // comment for why this stays this simple.
        var activeEvent = SeasonalEventCatalog.GetActiveEvent(today);
        var eventBonusCoins = activeEvent?.DailyRewardBonusCoins ?? 0;
        var coins = scheduleCoins + eventBonusCoins;

        var progress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        if (progress is null)
        {
            progress = new UserProgress { UserId = request.UserId };
            _db.UserProgresses.Add(progress);
        }

        progress.Xp += xp;
        progress.Coins += coins;

        _db.DailyRewardClaims.Add(new DailyRewardClaim
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ClaimDateUtc = today,
            ConsecutiveDayNumber = dayNumber,
            CoinsAwarded = coins,
            XpAwarded = xp,
        });

        await _db.SaveChangesAsync(cancellationToken);

        var result = new ClaimDailyRewardResultDto(
            dayNumber, coins, xp, progress.Xp, progress.Coins, activeEvent?.Name, eventBonusCoins);

        return Result.Success(result);
    }
}
