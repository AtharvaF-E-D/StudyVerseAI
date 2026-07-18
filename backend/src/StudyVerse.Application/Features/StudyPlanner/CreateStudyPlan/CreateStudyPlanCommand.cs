using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.StudyPlanner.CreateStudyPlan;

/// <summary>
/// Archives any existing Active plan for <paramref name="UserId"/>, calls the AI plan generator
/// (see <c>StudyPlanPromptBuilder</c>/<c>StudyPlanAiResponseParser</c>) for a day-by-day schedule
/// between today and <paramref name="ExamDate"/> (capped at <c>StudyPlanPromptBuilder.MaxPlanHorizonDays</c>),
/// and persists the new plan plus every generated <c>StudyPlanTask</c> row in one go.
/// </summary>
public sealed record CreateStudyPlanCommand(
    Guid UserId,
    DateOnly ExamDate,
    IReadOnlyList<string> Subjects,
    IReadOnlyList<string> WeakTopics,
    int HoursPerDayMinutes) : IRequest<Result<CreateStudyPlanResultDto>>;

public sealed record CreateStudyPlanResultDto(Guid PlanId, DateOnly ExamDate, int TotalTasks);
