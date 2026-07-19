namespace StudyVerse.Domain.Entities;

/// <summary>
/// One day's daily-login-reward claim. <see cref="ConsecutiveDayNumber"/> (1-7, see
/// <see cref="StudyVerse.Domain.Gamification.DailyRewardSchedule"/>) cycles back to 1 after 7 and
/// resets to 1 if there's a gap since the user's last claim — the same "compare stored date to
/// today, reset on a gap" reasoning <c>StreakService</c> uses for <see cref="UserProgress.LastActivityDateUtc"/>,
/// applied here to a reward-escalation counter instead of the activity streak itself (a user can
/// claim the daily reward without otherwise being "active" that day, so this is deliberately a
/// separate counter, not reuse of <see cref="UserProgress.CurrentStreakDays"/>). A unique index on
/// (UserId, ClaimDateUtc) prevents a second claim on the same UTC calendar date.
/// </summary>
public class DailyRewardClaim
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateOnly ClaimDateUtc { get; set; }

    public int ConsecutiveDayNumber { get; set; }

    public int CoinsAwarded { get; set; }

    public int XpAwarded { get; set; }

    public User? User { get; set; }
}
