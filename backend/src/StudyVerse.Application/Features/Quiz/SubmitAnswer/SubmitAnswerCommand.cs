using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Quiz.SubmitAnswer;

public sealed record SubmitAnswerCommand(Guid UserId, Guid SessionId, Guid QuestionId, int SelectedOptionIndex, int? TimeTakenMs)
    : IRequest<Result<SubmitAnswerResultDto>>;

public sealed record SubmitAnswerResultDto(
    bool IsCorrect,
    int CorrectOptionIndex,
    string Explanation,
    int XpEarnedThisAnswer,
    int ComboCount,
    int LivesRemaining,
    bool IsSessionComplete,
    QuizSessionSummaryDto? SessionSummary);

/// <summary>Only populated when the answer just submitted completed the session (all questions answered, or lives hit 0).</summary>
public sealed record QuizSessionSummaryDto(
    int TotalQuestions,
    int CorrectAnswers,
    int Score,
    int XpEarned,
    int CoinsEarned,
    int BestCombo,
    bool CompletedAllQuestions,
    bool RanOutOfLives,
    int DailyChallengeBonusXp,
    int DailyChallengeBonusCoins);
