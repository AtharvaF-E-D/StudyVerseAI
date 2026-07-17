using MediatR;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Quiz.GetQuizReview;

/// <summary>Only available for Completed sessions: every question with the user's answer, the correct answer, and the explanation.</summary>
public sealed record GetQuizReviewQuery(Guid UserId, Guid SessionId) : IRequest<Result<QuizReviewDto>>;

public sealed record QuizReviewDto(
    Guid SessionId,
    string Category,
    QuizDifficulty Difficulty,
    bool IsDailyChallenge,
    int Score,
    int XpEarned,
    int CoinsEarned,
    int BestCombo,
    IReadOnlyList<QuizReviewQuestionDto> Questions);

public sealed record QuizReviewQuestionDto(
    Guid QuestionId,
    int OrderIndex,
    string QuestionText,
    IReadOnlyList<string> Options,
    int CorrectOptionIndex,
    int? SelectedOptionIndex,
    bool? IsCorrect,
    string Explanation,
    int? TimeTakenMs);
