using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Common.Services;

/// <summary>
/// Placed directly in Application (rather than Infrastructure) because it only depends on other
/// Application abstractions (<see cref="IAppDbContext"/>, <see cref="IDateTimeProvider"/>) and has
/// no provider-specific dependency of its own.
/// </summary>
public sealed class StreakService : IStreakService
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StreakService(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task RecordActivityAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var progress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (progress is null)
        {
            progress = new UserProgress { UserId = userId };
            _db.UserProgresses.Add(progress);
        }

        var lastActivity = progress.LastActivityDateUtc;
        if (lastActivity is null)
        {
            progress.CurrentStreakDays = 1;
            progress.LastActivityDateUtc = today;
        }
        else if (lastActivity == today)
        {
            // Already recorded today; no-op.
        }
        else if (lastActivity == today.AddDays(-1))
        {
            progress.CurrentStreakDays++;
            progress.LastActivityDateUtc = today;
        }
        else
        {
            // A gap of 2+ days breaks the streak.
            progress.CurrentStreakDays = 1;
            progress.LastActivityDateUtc = today;
        }

        progress.LongestStreakDays = Math.Max(progress.LongestStreakDays, progress.CurrentStreakDays);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
