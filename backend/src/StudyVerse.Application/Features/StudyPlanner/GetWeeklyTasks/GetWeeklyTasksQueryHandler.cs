using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.StudyPlanner.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.StudyPlanner.GetWeeklyTasks;

public sealed class GetWeeklyTasksQueryHandler : IRequestHandler<GetWeeklyTasksQuery, Result<IReadOnlyList<StudyPlanTaskDto>>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetWeeklyTasksQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<StudyPlanTaskDto>>> Handle(GetWeeklyTasksQuery request, CancellationToken cancellationToken)
    {
        var plan = await _db.StudyPlans.FirstOrDefaultAsync(
            p => p.UserId == request.UserId && p.Status == StudyPlanStatus.Active, cancellationToken);

        if (plan is null)
        {
            return Result.Failure<IReadOnlyList<StudyPlanTaskDto>>("No active study plan found.", ResultErrorType.NotFound);
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var weekStart = request.WeekStartDate ?? MostRecentMonday(today);
        var weekEnd = weekStart.AddDays(6);

        var tasks = await _db.StudyPlanTasks
            .Where(t => t.PlanId == plan.Id && t.ScheduledDateUtc >= weekStart && t.ScheduledDateUtc <= weekEnd)
            .OrderBy(t => t.ScheduledDateUtc)
            .ThenBy(t => t.Subject)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<StudyPlanTaskDto>>(tasks.Select(StudyPlanMappings.ToDto).ToList());
    }

    /// <summary>The Monday on/before <paramref name="date"/> (returns <paramref name="date"/> itself
    /// when it's already a Monday).</summary>
    private static DateOnly MostRecentMonday(DateOnly date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
