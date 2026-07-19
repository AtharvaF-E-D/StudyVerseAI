namespace StudyVerse.Domain.Enums;

/// <summary>Difficulty tier of a <see cref="StudyVerse.Domain.Entities.CodingProblem"/>. A separate
/// enum from <see cref="QuizDifficulty"/> (even though the tiers are conceptually the same) — each
/// content-bank feature owns its own difficulty enum, the same way <see cref="QuizDifficulty"/>
/// itself isn't shared with <c>MockTestAttemptStatus</c>'s scoring.</summary>
public enum CodingDifficulty
{
    Easy = 0,
    Medium = 1,
    Hard = 2,
}
