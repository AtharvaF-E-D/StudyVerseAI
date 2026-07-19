using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Gamification.GetDailyRewardStatus;

public sealed record GetDailyRewardStatusQuery(Guid UserId) : IRequest<Result<DailyRewardStatusDto>>;
