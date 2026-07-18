namespace StudyVerse.Domain.Entities;

/// <summary>
/// A real AI-generated 3-4 paragraph digest summarizing the week's most significant cached news
/// stories across every category — shared across ALL users (unlike every other entity in this
/// feature), so <see cref="WeekStartDateUtc"/> alone (the Monday the ISO week starts, matching
/// <c>GetWeeklyTasksQuery</c>'s week-start convention) is unique and is the whole cache key: at most
/// one digest is ever generated per week, regardless of how many users request it.
/// </summary>
public class WeeklyDigest
{
    public Guid Id { get; set; }

    public DateOnly WeekStartDateUtc { get; set; }

    public string SummaryText { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; }
}
