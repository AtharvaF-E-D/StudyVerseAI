using MediatR;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.StartInterviewSession;

public sealed record StartInterviewSessionCommand(Guid UserId, InterviewQuestionType Type)
    : IRequest<Result<InterviewSessionDto>>;
