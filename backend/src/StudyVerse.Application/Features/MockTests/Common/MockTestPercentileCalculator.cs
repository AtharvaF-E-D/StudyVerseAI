namespace StudyVerse.Application.Features.MockTests.Common;

/// <summary>
/// Computes a submitted <see cref="StudyVerse.Domain.Entities.MockTestAttempt"/>'s
/// <c>PercentileRank</c> against every OTHER Submitted attempt for the same template — the
/// standard "mean of exclusive/inclusive rank" percentile formula: for a score S among N other
/// scores, percentile = (count strictly below S + 0.5 * count equal to S) / N * 100. Ties are
/// split down the middle rather than all landing on the same edge, and a perfect score among a
/// field of other perfect scores lands at 50 (not 100) since half the field, by definition,
/// scored the same rather than strictly worse.
///
/// Pulled out as a pure, DB-free function (mirrors <c>QuizScoring</c>/<c>LevelCalculator</c>) so
/// the formula — including its documented zero-prior-attempts edge case — is exercised directly by
/// unit tests without needing to seed a database.
/// </summary>
internal static class MockTestPercentileCalculator
{
    /// <summary>
    /// If there are zero other Submitted attempts for this template yet, this attempt is,
    /// definitionally, the top of an n=1 field — return 100 rather than an undefined/null value,
    /// so the first person to ever submit a given mock test template still sees a meaningful
    /// "you're in the top X%" result instead of a blank one.
    /// </summary>
    public static double Calculate(int score, IReadOnlyCollection<int> otherSubmittedScores)
    {
        if (otherSubmittedScores.Count == 0)
        {
            return 100d;
        }

        var strictlyLowerCount = otherSubmittedScores.Count(s => s < score);
        var equalCount = otherSubmittedScores.Count(s => s == score);

        return (strictlyLowerCount + 0.5 * equalCount) / otherSubmittedScores.Count * 100d;
    }
}
