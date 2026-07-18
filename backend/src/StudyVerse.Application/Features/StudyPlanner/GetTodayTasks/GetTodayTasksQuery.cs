using MediatR;
using StudyVerse.Application.Features.StudyPlanner.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.StudyPlanner.GetTodayTasks;

/// <summary>Runs the missed-task recovery pass (see <c>MissedTaskRecoveryService</c>) before
/// returning today's Pending/Completed tasks for the user's active plan.</summary>
public sealed record GetTodayTasksQuery(Guid UserId) : IRequest<Result<IReadOnlyList<StudyPlanTaskDto>>>;
