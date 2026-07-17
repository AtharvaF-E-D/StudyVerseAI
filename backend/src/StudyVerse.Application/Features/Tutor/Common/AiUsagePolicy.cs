using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Features.Tutor.Common;

/// <summary>
/// Shared daily-token-cap reset logic for the AI tutor. Mirrors the same "compare to stored date,
/// reset if different" pattern <c>StreakService</c> uses for <see cref="UserProgress.LastActivityDateUtc"/>.
/// </summary>
public static class AiUsagePolicy
{
    /// <summary>
    /// The effective tokens-used-today for read-only reporting (<c>GetAiUsageQueryHandler</c>):
    /// 0 if there's no progress row yet, or if the stored reset date isn't today (the day rolled
    /// over since it was last written) — without mutating anything.
    /// </summary>
    public static int GetTokensUsedToday(UserProgress? progress, DateOnly today) =>
        progress is not null && progress.AiUsageResetDateUtc == today ? progress.AiTokensUsedToday : 0;

    /// <summary>
    /// Resets <paramref name="progress"/>'s counter in place if the stored reset date isn't today.
    /// Used by <c>SendMessageCommandHandler</c> just before checking/incrementing usage. The
    /// caller is responsible for persisting (<c>SaveChangesAsync</c>).
    /// </summary>
    public static void ResetIfNewDay(UserProgress progress, DateOnly today)
    {
        if (progress.AiUsageResetDateUtc != today)
        {
            progress.AiTokensUsedToday = 0;
            progress.AiUsageResetDateUtc = today;
        }
    }
}
