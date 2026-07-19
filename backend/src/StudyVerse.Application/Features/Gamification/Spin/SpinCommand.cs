using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Gamification.Spin;

public sealed record SpinCommand(Guid UserId) : IRequest<Result<SpinResultDto>>;
