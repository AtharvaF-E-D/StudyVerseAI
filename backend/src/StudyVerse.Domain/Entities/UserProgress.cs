namespace StudyVerse.Domain.Entities;

/// <summary>
/// A user's gamification state: XP, coins, and daily-activity streak. One row per user, created
/// lazily (the first time activity is recorded or a challenge is completed) rather than at
/// registration time, since not every existing/new user has engaged with gamification yet.
/// </summary>
public class UserProgress
{
    /// <summary>Both the primary key and the foreign key to <see cref="User"/> (shared-key one-to-one).</summary>
    public Guid UserId { get; set; }

    public int Xp { get; set; }

    public int Coins { get; set; }

    public int CurrentStreakDays { get; set; }

    public int LongestStreakDays { get; set; }

    /// <summary>The last UTC calendar date the user "showed up" (signed in). Null until the first activity.</summary>
    public DateOnly? LastActivityDateUtc { get; set; }

    public User? User { get; set; }
}
