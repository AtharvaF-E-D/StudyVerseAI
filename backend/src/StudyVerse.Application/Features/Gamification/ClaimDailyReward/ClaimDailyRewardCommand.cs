using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Gamification.ClaimDailyReward;

public sealed record ClaimDailyRewardCommand(Guid UserId) : IRequest<Result<ClaimDailyRewardResultDto>>;
