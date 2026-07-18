using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.StudyPlanner.Common;

/// <summary>
/// The Study Planner's "smart rescheduling": a plain service class (not a new interface
/// abstraction — it only needs <see cref="IAppDbContext"/>/<see cref="IDateTimeProvider"/>, the same
/// reasoning as <c>StreakService</c>) that finds every still-<see cref="StudyPlanTaskStatus.Pending"/>
/// task in a user's active plan whose <see cref="StudyPlanTask.ScheduledDateUtc"/> has already
/// passed, marks it <see cref="StudyPlanTaskStatus.Missed"/>, and creates a same-shape replacement
/// task on the nearest future day that still has spare capacity under the plan's daily minute
/// budget. Called at the top of <c>GetTodayTasksQuery</c>/<c>GetActivePlanQuery</c> so a user never
/// has to manually "reschedule" anything — the plan self-heals the moment they next look at it.
/// </summary>
public sealed class MissedTaskRecoveryService
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MissedTaskRecoveryService(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>No-op if the user has no active plan, or nothing is overdue.</summary>
    public async Task RecoverAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var plan = await _db.StudyPlans.FirstOrDefaultAsync(
            p => p.UserId == userId && p.Status == StudyPlanStatus.Active, cancellationToken);

        if (plan is null)
        {
            return;
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var missedTasks = await _db.StudyPlanTasks
            .Where(t => t.PlanId == plan.Id && t.Status == StudyPlanTaskStatus.Pending && t.ScheduledDateUtc < today)
            .OrderBy(t => t.ScheduledDateUtc)
            .ToListAsync(cancellationToken);

        if (missedTasks.Count == 0)
        {
            return;
        }

        var tomorrow = today.AddDays(1);

        // Seed each future day's already-committed minutes from every task currently scheduled
        // there (any status - a day's real load is a day's real load regardless), so replacement
        // tasks never double-book a day that's already full. Kept as an in-memory running total,
        // updated as each missed task below claims a slot, so a batch of several misses processed
        // in the same pass don't all pile onto the same first-available day.
        var futureTasks = await _db.StudyPlanTasks
            .Where(t => t.PlanId == plan.Id && t.ScheduledDateUtc >= tomorrow)
            .Select(t => new { t.ScheduledDateUtc, t.DurationMinutes })
            .ToListAsync(cancellationToken);

        var dayLoadMinutes = futureTasks
            .GroupBy(t => t.ScheduledDateUtc)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.DurationMinutes));

        // "The day immediately before the exam" - falling back further (to the exam date itself, or
        // to tomorrow) only in the edge cases where the exam is imminent enough that there's no room
        // strictly before it left to fall back to.
        var fallbackDate = plan.ExamDate.AddDays(-1);
        if (fallbackDate < tomorrow)
        {
            fallbackDate = plan.ExamDate < tomorrow ? tomorrow : plan.ExamDate;
        }

        foreach (var missed in missedTasks)
        {
            missed.Status = StudyPlanTaskStatus.Missed;

            // Carry forward the very first date this session was due, even if it's already been
            // bumped forward once before (i.e. this "missed" row is itself an earlier replacement).
            var originalDate = missed.OriginalScheduledDateUtc ?? missed.ScheduledDateUtc;

            DateOnly? targetDate = null;
            for (var day = tomorrow; day <= plan.ExamDate; day = day.AddDays(1))
            {
                var currentLoad = dayLoadMinutes.GetValueOrDefault(day);
                if (currentLoad + missed.DurationMinutes <= plan.HoursPerDayMinutes)
                {
                    targetDate = day;
                    break;
                }
            }

            // Every day through the exam is already full - the exam is coming regardless, so pile
            // this makeup session onto the day right before it rather than silently dropping it.
            targetDate ??= fallbackDate;

            dayLoadMinutes[targetDate.Value] = dayLoadMinutes.GetValueOrDefault(targetDate.Value) + missed.DurationMinutes;

            _db.StudyPlanTasks.Add(new StudyPlanTask
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                ScheduledDateUtc = targetDate.Value,
                Subject = missed.Subject,
                Topic = missed.Topic,
                DurationMinutes = missed.DurationMinutes,
                IsWeakTopic = missed.IsWeakTopic,
                Status = StudyPlanTaskStatus.Pending,
                OriginalScheduledDateUtc = originalDate,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
