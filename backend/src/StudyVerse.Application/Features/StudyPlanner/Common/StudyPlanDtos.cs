using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.StudyPlanner.Common;

public sealed record StudyPlanTaskDto(
    Guid Id,
    DateOnly ScheduledDateUtc,
    string Subject,
    string Topic,
    int DurationMinutes,
    bool IsWeakTopic,
    StudyPlanTaskStatus Status,
    DateTime? CompletedAtUtc,
    DateOnly? OriginalScheduledDateUtc);

public sealed record ActiveStudyPlanDto(
    Guid PlanId,
    DateOnly ExamDate,
    int DaysRemaining,
    IReadOnlyList<string> Subjects,
    IReadOnlyList<string> WeakTopics,
    int HoursPerDayMinutes,
    int TotalTasks,
    int CompletedTasks,
    int MissedTasks,
    double ProgressPercent);
