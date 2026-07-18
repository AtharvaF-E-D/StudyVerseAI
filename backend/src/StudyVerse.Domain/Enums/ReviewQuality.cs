namespace StudyVerse.Domain.Enums;

/// <summary>
/// The 4-point review-quality scale the client submits after grading its own recall of a
/// flashcard, mapped onto specific points of the standard SM-2 algorithm's 0-5 quality scale
/// (see <see cref="StudyVerse.Domain.SpacedRepetition.Sm2Scheduler"/>). This is a straight subset,
/// not a translation table: Again/Hard/Good/Easy already sit at SM-2 quality points 0/3/4/5, so
/// this enum's underlying int IS the SM-2 quality value passed straight to the scheduler.
///
/// SM-2's q=1 and q=2 ("incorrect, but the answer felt familiar") are deliberately not exposed
/// client-side — both fall into the same "reset repetitions" branch as <see cref="Again"/> (any
/// q &lt; 3) anyway, just with a slightly smaller ease-factor penalty, which isn't worth a 6-point
/// UI for a mobile flashcard-review screen.
/// </summary>
public enum ReviewQuality
{
    /// <summary>Forgot the card entirely. SM-2 q=0: resets repetitions/interval to 0/1.</summary>
    Again = 0,

    /// <summary>Recalled it, but it was a struggle. SM-2 q=3: lowest ease-factor increment, no reset.</summary>
    Hard = 3,

    /// <summary>Recalled it with some effort. SM-2 q=4: the "normal" successful review.</summary>
    Good = 4,

    /// <summary>Recalled it instantly. SM-2 q=5: the largest ease-factor boost.</summary>
    Easy = 5,
}
