using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Features.Gamification.Spin;

/// <summary>
/// Spins the daily prize wheel once per UTC calendar day. The prize is picked by drawing a single
/// uniform roll in [0, <see cref="SpinPrizeCatalog.TotalWeight"/>) via <see cref="IRandomProvider"/>
/// (a real, mockable-in-tests wrapper over <c>Random.Shared</c>) and mapping it through
/// <see cref="SpinWheelSelector"/>.
/// </summary>
public sealed class SpinCommandHandler : IRequestHandler<SpinCommand, Result<SpinResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IRandomProvider _randomProvider;

    public SpinCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, IRandomProvider randomProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _randomProvider = randomProvider;
    }

    public async Task<Result<SpinResultDto>> Handle(SpinCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<SpinResultDto>("User not found.", ResultErrorType.NotFound);
        }

        var now = _dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Pre-check for a friendly error message; the unique index on (UserId, SpinDateUtc) is the
        // actual source of truth guarding against a concurrent second spin the same day.
        var alreadySpunToday = await _db.SpinResults.AnyAsync(
            s => s.UserId == request.UserId && s.SpinDateUtc == today,
            cancellationToken);

        if (alreadySpunToday)
        {
            return Result.Failure<SpinResultDto>(
                "You've already spun the wheel today. Come back tomorrow!",
                ResultErrorType.Conflict);
        }

        var roll = _randomProvider.Next(0, SpinPrizeCatalog.TotalWeight);
        var prize = SpinWheelSelector.SelectPrize(roll);

        var progress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        if (progress is null)
        {
            progress = new UserProgress { UserId = request.UserId };
            _db.UserProgresses.Add(progress);
        }

        progress.Xp += prize.XpAwarded;
        progress.Coins += prize.CoinsAwarded;

        _db.SpinResults.Add(new SpinResult
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SpinDateUtc = today,
            SpunAtUtc = now,
            PrizeLabel = prize.Label,
            CoinsAwarded = prize.CoinsAwarded,
            XpAwarded = prize.XpAwarded,
        });

        await _db.SaveChangesAsync(cancellationToken);

        var result = new SpinResultDto(prize.Label, prize.CoinsAwarded, prize.XpAwarded, progress.Xp, progress.Coins);

        return Result.Success(result);
    }
}
