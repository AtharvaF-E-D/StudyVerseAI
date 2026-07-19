using StudyVerse.Domain.Enums;

namespace StudyVerse.Domain.CodingPractice;

/// <summary>
/// Pure XP/coin reward rules for a solved <see cref="Entities.CodingProblem"/>, shared by
/// <c>SubmitCodeCommandHandler</c> and its tests — the same "pure function of difficulty" shape as
/// <see cref="Quiz.QuizScoring"/>. Awarded exactly once per problem, on the user's first-ever
/// <see cref="CodeSubmissionStatus.Accepted"/> submission for it (see that handler's doc comment).
/// </summary>
public static class CodingScoring
{
    public static int GetXpReward(CodingDifficulty difficulty) => difficulty switch
    {
        CodingDifficulty.Easy => 15,
        CodingDifficulty.Medium => 25,
        CodingDifficulty.Hard => 40,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unknown coding difficulty."),
    };

    public static int GetCoinReward(CodingDifficulty difficulty) => difficulty switch
    {
        CodingDifficulty.Easy => 3,
        CodingDifficulty.Medium => 5,
        CodingDifficulty.Hard => 8,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unknown coding difficulty."),
    };
}
