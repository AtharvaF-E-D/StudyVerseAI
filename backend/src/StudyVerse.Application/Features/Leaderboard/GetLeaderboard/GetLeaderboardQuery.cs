using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Leaderboard.GetLeaderboard;

public sealed record GetLeaderboardQuery(Guid UserId, int Take) : IRequest<Result<IReadOnlyList<LeaderboardEntryDto>>>;
