using MediatR;
using StudyVerse.Application.Features.Quiz.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Quiz.GetQuizSession;

/// <summary>For resume-after-app-restart: current question index, lives, combo, which power-ups
/// were used, and the current question's text+options (never the answer).</summary>
public sealed record GetQuizSessionQuery(Guid UserId, Guid SessionId) : IRequest<Result<QuizSessionStateDto>>;

public sealed record QuizSessionStateDto(
    Guid SessionId,
    string Category,
    QuizDifficulty Difficulty,
    QuizSessionStatus Status,
    bool IsDailyChallenge,
    int CurrentQuestionIndex,
    int TotalQuestions,
    int LivesRemaining,
    int ComboCount,
    int BestCombo,
    int Score,
    PowerUpsAvailableDto PowerUpsAvailable,
    /// <summary>Null once the session is Completed/Abandoned — there's no "current question" left to resume.</summary>
    QuizQuestionOptionsDto? CurrentQuestion);
