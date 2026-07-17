namespace StudyVerse.Domain.Gamification;

/// <summary>Pure XP-to-level formula, shared by the dashboard and the challenge-completion result.</summary>
public static class LevelCalculator
{
    public static int GetLevel(int xp) => (int)Math.Floor(Math.Sqrt(xp / 50.0)) + 1;
}
