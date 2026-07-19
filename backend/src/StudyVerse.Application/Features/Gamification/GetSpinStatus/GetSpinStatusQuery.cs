using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Gamification.GetSpinStatus;

public sealed record GetSpinStatusQuery(Guid UserId) : IRequest<Result<SpinStatusDto>>;
