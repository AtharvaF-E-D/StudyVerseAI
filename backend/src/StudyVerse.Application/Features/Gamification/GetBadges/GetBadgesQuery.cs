using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Gamification.GetBadges;

public sealed record GetBadgesQuery(Guid UserId) : IRequest<Result<GetBadgesResultDto>>;
