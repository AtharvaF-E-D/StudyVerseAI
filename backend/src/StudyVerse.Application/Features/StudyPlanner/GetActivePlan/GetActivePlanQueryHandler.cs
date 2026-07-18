using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.StudyPlanner.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.StudyPlanner.GetActivePlan;

public sealed class GetActivePlanQueryHandler : IRequestHandler<GetActivePlanQuery, Result<ActiveStudyPlanDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly MissedTaskRecoveryService _missedTaskRecoveryService;

    public GetActivePlanQueryHandler(
        IAppDbContext db, IDateTimeProvider dateTimeProvider, MissedTaskRecoveryService missedTaskRecoveryService)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _missedTaskRecoveryService = missedTaskRecoveryService;
    }

    public async Task<Result<ActiveStudyPlanDto>> Handle(GetActivePlanQuery request, CancellationToken cancellationToken)
    {
        // Self-heal any overdue tasks before computing progress/missed counts below, so this
        // summary is never stale - see MissedTaskRecoveryService's doc comment.
        await _missedTaskRecoveryService.RecoverAsync(request.UserId, cancellationToken);

        var plan = await _db.StudyPlans.FirstOrDefaultAsync(
            p => p.UserId == request.UserId && p.Status == StudyPlanStatus.Active, cancellationToken);

        if (plan is null)
        {
            return Result.Failure<ActiveStudyPlanDto>("No active study plan found.", ResultErrorType.NotFound);
        }

        var totalTasks = await _db.StudyPlanTasks.CountAsync(t => t.PlanId == plan.Id, cancellationToken);
        var completedTasks = await _db.StudyPlanTasks.CountAsync(
            t => t.PlanId == plan.Id && t.Status == StudyPlanTaskStatus.Completed, cancellationToken);
        var missedTasks = await _db.StudyPlanTasks.CountAsync(
            t => t.PlanId == plan.Id && t.Status == StudyPlanTaskStatus.Missed, cancellationToken);

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var daysRemaining = Math.Max(0, plan.ExamDate.DayNumber - today.DayNumber);
        var progressPercent = totalTasks == 0 ? 0 : Math.Round(100.0 * completedTasks / totalTasks, 1);

        var dto = new ActiveStudyPlanDto(
            plan.Id,
            plan.ExamDate,
            daysRemaining,
            StudyPlanJsonHelper.Deserialize(plan.SubjectsJson),
            StudyPlanJsonHelper.Deserialize(plan.WeakTopicsJson),
            plan.HoursPerDayMinutes,
            totalTasks,
            completedTasks,
            missedTasks,
            progressPercent);

        return Result.Success(dto);
    }
}
