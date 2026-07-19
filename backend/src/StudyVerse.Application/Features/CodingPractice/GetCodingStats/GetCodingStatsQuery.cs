using MediatR;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CodingPractice.GetCodingStats;

public sealed record GetCodingStatsQuery(Guid UserId) : IRequest<Result<CodingStatsDto>>;
