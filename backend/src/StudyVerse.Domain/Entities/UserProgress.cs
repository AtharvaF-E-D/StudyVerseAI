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

    /// <summary>
    /// Total prompt+completion tokens the AI tutor has used for this user on
    /// <see cref="AiUsageResetDateUtc"/>. Reset to 0 the next time it's read/written on a
    /// different UTC calendar date (see <c>AiUsagePolicy</c>), mirroring the same
    /// "compare to stored date, reset if different" pattern <c>StreakService</c> uses for
    /// <see cref="LastActivityDateUtc"/> — added here rather than a sibling entity since it's
    /// another simple daily counter on the same one-row-per-user gamification/usage record.
    /// </summary>
    public int AiTokensUsedToday { get; set; }

    /// <summary>The UTC calendar date <see cref="AiTokensUsedToday"/> was last reset for. Null until the first AI tutor message.</summary>
    public DateOnly? AiUsageResetDateUtc { get; set; }

    public User? User { get; set; }
}
