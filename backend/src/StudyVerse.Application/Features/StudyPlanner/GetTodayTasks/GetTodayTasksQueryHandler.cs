using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.StudyPlanner.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.StudyPlanner.GetTodayTasks;

public sealed class GetTodayTasksQueryHandler : IRequestHandler<GetTodayTasksQuery, Result<IReadOnlyList<StudyPlanTaskDto>>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly MissedTaskRecoveryService _missedTaskRecoveryService;

    public GetTodayTasksQueryHandler(
        IAppDbContext db, IDateTimeProvider dateTimeProvider, MissedTaskRecoveryService missedTaskRecoveryService)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _missedTaskRecoveryService = missedTaskRecoveryService;
    }

    public async Task<Result<IReadOnlyList<StudyPlanTaskDto>>> Handle(GetTodayTasksQuery request, CancellationToken cancellationToken)
    {
        // Self-heal any overdue tasks first - a task that was due yesterday and never touched
        // becomes Missed plus a fresh replacement elsewhere before we even look at "today".
        await _missedTaskRecoveryService.RecoverAsync(request.UserId, cancellationToken);

        var plan = await _db.StudyPlans.FirstOrDefaultAsync(
            p => p.UserId == request.UserId && p.Status == StudyPlanStatus.Active, cancellationToken);

        if (plan is null)
        {
            return Result.Failure<IReadOnlyList<StudyPlanTaskDto>>("No active study plan found.", ResultErrorType.NotFound);
        }

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var tasks = await _db.StudyPlanTasks
            .Where(t => t.PlanId == plan.Id
                        && t.ScheduledDateUtc == today
                        && (t.Status == StudyPlanTaskStatus.Pending || t.Status == StudyPlanTaskStatus.Completed))
            .OrderBy(t => t.Subject)
            .ThenBy(t => t.Topic)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<StudyPlanTaskDto>>(tasks.Select(StudyPlanMappings.ToDto).ToList());
    }
}
