namespace StudyVerse.Application.Features.Leaderboard;

public sealed record LeaderboardEntryDto(Guid UserId, string DisplayName, int Xp, int Rank);
