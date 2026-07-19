using MediatR;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewStats;

public sealed record GetInterviewStatsQuery(Guid UserId) : IRequest<Result<InterviewStatsDto>>;
