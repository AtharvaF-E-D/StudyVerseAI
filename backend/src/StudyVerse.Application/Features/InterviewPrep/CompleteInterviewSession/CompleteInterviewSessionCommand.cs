using MediatR;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.InterviewPrep.CompleteInterviewSession;

public sealed record CompleteInterviewSessionCommand(Guid UserId, Guid SessionId) : IRequest<Result<InterviewSessionDto>>;
