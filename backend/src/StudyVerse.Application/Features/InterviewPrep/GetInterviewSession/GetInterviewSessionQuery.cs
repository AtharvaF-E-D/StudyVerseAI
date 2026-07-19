using MediatR;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewSession;

public sealed record GetInterviewSessionQuery(Guid UserId, Guid SessionId) : IRequest<Result<InterviewSessionDto>>;
