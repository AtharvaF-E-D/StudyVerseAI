using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Quiz.AbandonQuizSession;

public sealed record AbandonQuizSessionCommand(Guid UserId, Guid SessionId) : IRequest<Result>;
