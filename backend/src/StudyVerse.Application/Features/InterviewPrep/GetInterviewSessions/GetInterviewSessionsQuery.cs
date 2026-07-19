using MediatR;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewSessions;

public sealed record GetInterviewSessionsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<InterviewSessionSummaryDto>>>;
