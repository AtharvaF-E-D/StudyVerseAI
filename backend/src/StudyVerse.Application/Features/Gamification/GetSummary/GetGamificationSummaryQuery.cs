using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Gamification.GetSummary;

public sealed record GetGamificationSummaryQuery(Guid UserId) : IRequest<Result<GamificationSummaryDto>>;
