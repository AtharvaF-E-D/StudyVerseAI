using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Features.StudyPlanner.Common;

internal static class StudyPlanMappings
{
    public static StudyPlanTaskDto ToDto(StudyPlanTask task) => new(
        task.Id,
        task.ScheduledDateUtc,
        task.Subject,
        task.Topic,
        task.DurationMinutes,
        task.IsWeakTopic,
        task.Status,
        task.CompletedAtUtc,
        task.OriginalScheduledDateUtc);
}
