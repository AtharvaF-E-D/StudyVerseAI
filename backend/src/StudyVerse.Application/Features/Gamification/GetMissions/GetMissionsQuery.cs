using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Gamification.GetMissions;

public sealed record GetMissionsQuery(Guid UserId) : IRequest<Result<GetMissionsResultDto>>;
