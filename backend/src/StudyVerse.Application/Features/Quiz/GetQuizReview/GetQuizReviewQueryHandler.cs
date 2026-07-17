using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Quiz.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Quiz.GetQuizReview;

public sealed class GetQuizReviewQueryHandler : IRequestHandler<GetQuizReviewQuery, Result<QuizReviewDto>>
{
    private readonly IAppDbContext _db;

    public GetQuizReviewQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<QuizReviewDto>> Handle(GetQuizReviewQuery request, CancellationToken cancellationToken)
    {
        var session = await _db.QuizSessions.FirstOrDefaultAsync(
            s => s.Id == request.SessionId && s.UserId == request.UserId,
            cancellationToken);

        if (session is null)
        {
            return Result.Failure<QuizReviewDto>("Quiz session not found.", ResultErrorType.NotFound);
        }

        if (session.Status != QuizSessionStatus.Completed)
        {
            return Result.Failure<QuizReviewDto>(
                "A review is only available for completed quiz sessions.",
                ResultErrorType.Validation);
        }

        var questionRows = await (
            from sq in _db.QuizSessionQuestions
            join q in _db.QuizQuestions on sq.QuestionId equals q.Id
            where sq.SessionId == session.Id
            orderby sq.OrderIndex
            select new { sq, q }
        ).ToListAsync(cancellationToken);

        var questions = questionRows
            .Select(row => new QuizReviewQuestionDto(
                row.q.Id,
                row.sq.OrderIndex,
                row.q.QuestionText,
                QuizMapping.GetOptions(row.q),
                row.q.CorrectOptionIndex,
                row.sq.SelectedOptionIndex,
                row.sq.IsCorrect,
                row.q.Explanation,
                row.sq.TimeTakenMs))
            .ToList();

        var result = new QuizReviewDto(
            session.Id,
            session.Category,
            session.Difficulty,
            session.IsDailyChallenge,
            session.Score,
            session.XpEarned,
            session.CoinsEarned,
            session.BestComboThisSession,
            questions);

        return Result.Success(result);
    }
}
