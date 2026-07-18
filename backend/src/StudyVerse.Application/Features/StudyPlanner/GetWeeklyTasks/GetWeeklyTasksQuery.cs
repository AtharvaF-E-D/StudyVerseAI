using MediatR;
using StudyVerse.Application.Features.StudyPlanner.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.StudyPlanner.GetWeeklyTasks;

/// <summary>Tasks for the user's active plan across a 7-day window starting at
/// <paramref name="WeekStartDate"/> (default: the most recent Monday on/before today). Returns a
/// flat list (each item carries its own <c>ScheduledDateUtc</c>) rather than nesting by day -
/// simpler than server-side grouping, and just as easy for the client to group itself. Unlike
/// <c>GetTodayTasksQuery</c>, all statuses are included (Missed/Rescheduled too), since a weekly
/// overview is exactly where seeing what got missed is useful context.</summary>
public sealed record GetWeeklyTasksQuery(Guid UserId, DateOnly? WeekStartDate) : IRequest<Result<IReadOnlyList<StudyPlanTaskDto>>>;
