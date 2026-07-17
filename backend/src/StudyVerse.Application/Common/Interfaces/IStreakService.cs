namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Records that a user showed up "today" (UTC) and maintains their daily-activity streak.
/// Called from every real sign-in flow (login, OTP login, Google, Apple) — never from token
/// refresh, which renews a session rather than representing a new activity event.
/// </summary>
public interface IStreakService
{
    Task RecordActivityAsync(Guid userId, CancellationToken cancellationToken = default);
}
