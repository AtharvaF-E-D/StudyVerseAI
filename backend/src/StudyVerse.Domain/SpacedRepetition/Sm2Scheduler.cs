namespace StudyVerse.Domain.SpacedRepetition;

/// <summary>
/// A pure implementation of the standard SuperMemo SM-2 spaced-repetition algorithm (Piotr
/// Wozniak, 1987/1990) — the same formulas used by Anki and most flashcard apps. Takes a card's
/// current scheduling state plus a 0-5 review-quality rating and returns its next scheduling
/// state. No side effects, no persistence — <c>ReviewCardCommandHandler</c> is the only caller and
/// owns loading/saving the <see cref="Domain.Entities.Flashcard"/> around this call.
///
/// The algorithm, verbatim:
/// <code>
/// if q &gt;= 3 (correct response):
///     if repetitions == 0: interval = 1
///     elif repetitions == 1: interval = 6
///     else: interval = round(interval * easeFactor)
///     repetitions += 1
/// else (incorrect response):
///     repetitions = 0
///     interval = 1
///
/// easeFactor += 0.1 - (5 - q) * (0.08 + (5 - q) * 0.02)
/// easeFactor = max(easeFactor, 1.3)   // documented SM-2 floor
/// </code>
/// Note the ease-factor adjustment applies unconditionally (even on a reset), exactly as SM-2
/// specifies — a "Hard" or "Again" review still nudges EaseFactor down even though repetitions/
/// interval reset separately.
/// </summary>
public static class Sm2Scheduler
{
    /// <summary>SM-2's documented ease-factor floor — an ease factor is never allowed to drop
    /// below this, no matter how many consecutive "Again"/"Hard" reviews a card receives.</summary>
    public const double MinEaseFactor = 1.3;

    /// <summary>The ease factor a brand-new card starts at, per the SM-2 spec.</summary>
    public const double InitialEaseFactor = 2.5;

    /// <summary>
    /// Computes the next scheduling state for one review. <paramref name="quality"/> must be in
    /// [0, 5] (the standard client-facing <see cref="Enums.ReviewQuality"/> values 0/3/4/5 all
    /// satisfy this).
    /// </summary>
    /// <param name="current">The card's scheduling state going into this review.</param>
    /// <param name="quality">The SM-2 quality rating for this review, 0 (total blank) to 5 (perfect recall).</param>
    /// <param name="today">The review date, used to compute the absolute next-review date from the new interval.</param>
    public static Sm2ScheduleResult Schedule(Sm2CardState current, int quality, DateOnly today)
    {
        if (quality is < 0 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(quality), quality, "SM-2 quality must be between 0 and 5.");
        }

        int newRepetitions;
        int newIntervalDays;

        if (quality >= 3)
        {
            newIntervalDays = current.Repetitions switch
            {
                0 => 1,
                1 => 6,
                _ => (int)Math.Round(current.IntervalDays * current.EaseFactor, MidpointRounding.AwayFromZero),
            };
            newRepetitions = current.Repetitions + 1;
        }
        else
        {
            newRepetitions = 0;
            newIntervalDays = 1;
        }

        var newEaseFactor = current.EaseFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
        if (newEaseFactor < MinEaseFactor)
        {
            newEaseFactor = MinEaseFactor;
        }

        return new Sm2ScheduleResult(newEaseFactor, newIntervalDays, newRepetitions, today.AddDays(newIntervalDays));
    }
}

/// <summary>A flashcard's SM-2 scheduling state going into a review.</summary>
public readonly record struct Sm2CardState(double EaseFactor, int IntervalDays, int Repetitions);

/// <summary>The updated SM-2 scheduling state coming out of a review, including the absolute next review date.</summary>
public readonly record struct Sm2ScheduleResult(double EaseFactor, int IntervalDays, int Repetitions, DateOnly NextReviewDateUtc);
