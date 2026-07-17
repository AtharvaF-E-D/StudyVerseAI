using MediatR;
using StudyVerse.Application.Features.Quiz.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Quiz.StartQuizSession;

public sealed record StartQuizSessionCommand(Guid UserId, string Category, QuizDifficulty Difficulty, bool IsDailyChallenge)
    : IRequest<Result<StartQuizSessionResultDto>>;

public sealed record StartQuizSessionResultDto(
    Guid SessionId,
    IReadOnlyList<QuizQuestionOptionsDto> Questions,
    int LivesRemaining,
    PowerUpsAvailableDto PowerUpsAvailable,
    int TotalQuestions);
