using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Quiz.GetQuizStats;

public sealed record GetQuizStatsQuery(Guid UserId) : IRequest<Result<QuizStatsDto>>;

public sealed record QuizStatsDto(
    int TotalSessionsPlayed,
    int TotalQuestionsAnswered,
    int TotalCorrectAnswers,
    double AccuracyPercent,
    int BestComboEver,
    IReadOnlyList<QuizCategoryStatsDto> CategoryBreakdown);

public sealed record QuizCategoryStatsDto(string Category, int QuestionsAnswered, int QuestionsCorrect);
