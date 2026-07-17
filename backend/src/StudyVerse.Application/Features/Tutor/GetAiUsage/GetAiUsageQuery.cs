using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Tutor.GetAiUsage;

public sealed record GetAiUsageQuery(Guid UserId) : IRequest<Result<AiUsageDto>>;
