using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.Quiz;

/// <summary>
/// Pure scoring rules for a Rapid Fire Quiz session, shared by <c>SubmitAnswerCommandHandler</c>
/// and its tests. Deliberately simple (no time-based bonuses, no difficulty-curve tuning) per the
/// phase's time-boxing note — the core loop (lives/combo/scoring/review) is the priority.
/// </summary>
public static class QuizScoring
{
    public const int QuestionsPerSession = 10;

    public const int StartingLives = 3;

    /// <summary>Flat bonus (on top of normal per-question scoring) awarded once when a daily-challenge session completes.</summary>
    public const int DailyChallengeBonusXp = 25;

    public const int DailyChallengeBonusCoins = 10;

    private const int MaxComboForMultiplier = 5;
    private const double ComboMultiplierStep = 0.1;

    public static int GetBaseXp(QuizDifficulty difficulty) => difficulty switch
    {
        QuizDifficulty.Easy => 10,
        QuizDifficulty.Medium => 15,
        QuizDifficulty.Hard => 25,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unknown quiz difficulty."),
    };

    /// <summary>Flat coin reward for a correct answer. Unlike XP, this is never combo-scaled.</summary>
    public static int GetCoinReward(QuizDifficulty difficulty) => difficulty switch
    {
        QuizDifficulty.Easy => 2,
        QuizDifficulty.Medium => 3,
        QuizDifficulty.Hard => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unknown quiz difficulty."),
    };

    /// <summary>
    /// 1.0x with no active streak, +0.1x per consecutive correct answer, capped at 1.5x at a
    /// streak of 5 or more. <paramref name="comboCountAfterThisAnswer"/> is the
    /// <see cref="Entities.QuizSession.ComboCount"/> value AFTER incrementing for the correct
    /// answer just given, so the 5th consecutive correct answer itself already earns the full
    /// 1.5x multiplier (not just the 6th).
    /// </summary>
    public static double GetComboMultiplier(int comboCountAfterThisAnswer) =>
        1.0 + Math.Min(comboCountAfterThisAnswer, MaxComboForMultiplier) * ComboMultiplierStep;

    /// <summary>Base XP for the difficulty, scaled by the combo multiplier and rounded to the nearest whole XP point.</summary>
    public static int GetXpForCorrectAnswer(QuizDifficulty difficulty, int comboCountAfterThisAnswer) =>
        (int)Math.Round(GetBaseXp(difficulty) * GetComboMultiplier(comboCountAfterThisAnswer), MidpointRounding.AwayFromZero);
}
