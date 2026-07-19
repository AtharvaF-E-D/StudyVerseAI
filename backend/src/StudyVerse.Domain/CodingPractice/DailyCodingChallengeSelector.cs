namespace StudyVerse.Domain.CodingPractice;

/// <summary>
/// Deterministically picks one problem id per UTC calendar day from the full seeded problem pool —
/// the same <c>(dayOfYear + year) % N</c> rotation style as
/// <see cref="StudyVerse.Domain.Gamification.DailyChallengeSelector"/> and
/// <see cref="StudyVerse.Domain.Quiz.DailyQuizSelector"/>. Unlike those two (which embed their own
/// fixed in-Domain catalogs), the coding problem bank is a real DB table seeded from Infrastructure
/// (<c>CodingProblemSeedData</c>), which Domain can't reference — so this takes the caller's
/// already-fetched, stably-ordered list of problem ids as a parameter instead of owning the catalog
/// itself. <c>GetDailyCodingChallengeQueryHandler</c> supplies that list ordered by <c>Id</c> (the
/// seeded problems' ids are fixed hardcoded GUIDs, so that ordering never changes across
/// migrations, keeping the rotation stable forever).
/// </summary>
public static class DailyCodingChallengeSelector
{
    public static Guid GetTodaysProblemId(IReadOnlyList<Guid> orderedProblemIds, DateOnly today)
    {
        if (orderedProblemIds.Count == 0)
        {
            throw new ArgumentException("The problem pool must not be empty.", nameof(orderedProblemIds));
        }

        var seed = (today.DayOfYear + today.Year) % orderedProblemIds.Count;
        return orderedProblemIds[seed];
    }
}
