namespace StudyVerse.Domain.Entities;

/// <summary>
/// An in-app notification for a user (e.g. the welcome message seeded at registration). There is
/// no general notification-sending feature yet — rows are created ad hoc by specific flows.
/// </summary>
public class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ReadAtUtc { get; set; }

    public bool IsRead => ReadAtUtc.HasValue;

    public User? User { get; set; }
}
