using MediatR;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.InterviewPrep.SubmitAnswer;

public sealed record SubmitAnswerCommand(Guid UserId, Guid SessionId, Guid QuestionId, string AnswerText)
    : IRequest<Result<SubmitAnswerResultDto>>;
