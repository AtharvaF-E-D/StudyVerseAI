using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Quiz.GetQuizStats;

public sealed class GetQuizStatsQueryHandler : IRequestHandler<GetQuizStatsQuery, Result<QuizStatsDto>>
{
    private readonly IAppDbContext _db;

    public GetQuizStatsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<QuizStatsDto>> Handle(GetQuizStatsQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<QuizStatsDto>("User not found.", ResultErrorType.NotFound);
        }

        // Answered questions across ALL of the user's sessions (any status) — an abandoned session
        // still recorded real answers up to the point of quitting, and those should count toward
        // lifetime accuracy stats even though the session itself isn't "played to completion".
        var answeredQuestions = await (
            from sq in _db.QuizSessionQuestions
            join s in _db.QuizSessions on sq.SessionId equals s.Id
            where s.UserId == request.UserId && sq.AnsweredAtUtc != null
            select new { s.Category, sq.IsCorrect }
        ).ToListAsync(cancellationToken);

        var totalQuestionsAnswered = answeredQuestions.Count;
        var totalCorrectAnswers = answeredQuestions.Count(a => a.IsCorrect == true);
        var accuracyPercent = totalQuestionsAnswered == 0
            ? 0d
            : Math.Round(100.0 * totalCorrectAnswers / totalQuestionsAnswered, 1);

        var categoryBreakdown = answeredQuestions
            .GroupBy(a => a.Category)
            .Select(g => new QuizCategoryStatsDto(g.Key, g.Count(), g.Count(a => a.IsCorrect == true)))
            .OrderBy(c => c.Category)
            .ToList();

        var totalSessionsPlayed = await _db.QuizSessions.CountAsync(
            s => s.UserId == request.UserId && s.Status == QuizSessionStatus.Completed,
            cancellationToken);

        var bestComboEver = await _db.QuizSessions
            .Where(s => s.UserId == request.UserId)
            .Select(s => (int?)s.BestComboThisSession)
            .MaxAsync(cancellationToken) ?? 0;

        var result = new QuizStatsDto(
            totalSessionsPlayed,
            totalQuestionsAnswered,
            totalCorrectAnswers,
            accuracyPercent,
            bestComboEver,
            categoryBreakdown);

        return Result.Success(result);
    }
}
