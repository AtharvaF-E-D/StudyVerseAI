namespace StudyVerse.Domain.Entities;

/// <summary>
/// A single AI-tutor chat thread belonging to one user. <see cref="Title"/> starts as the
/// placeholder "New conversation" and is set to a short truncation of the user's first message
/// once the first exchange in the thread completes. <see cref="UpdatedAtUtc"/> tracks chat
/// activity (bumped on every new message, not on organizational actions like bookmarking) so the
/// conversation list can be sorted by "most recently talked about".
/// </summary>
public class Conversation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsBookmarked { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public User? User { get; set; }

    public List<Message> Messages { get; set; } = [];
}
