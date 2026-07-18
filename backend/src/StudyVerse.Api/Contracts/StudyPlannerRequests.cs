namespace StudyVerse.Api.Contracts;

public sealed record CreateStudyPlanRequest(
    DateOnly ExamDate,
    IReadOnlyList<string> Subjects,
    IReadOnlyList<string> WeakTopics,
    int HoursPerDayMinutes);
