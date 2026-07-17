using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Quiz.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Quiz.GetQuizSession;

public sealed class GetQuizSessionQueryHandler : IRequestHandler<GetQuizSessionQuery, Result<QuizSessionStateDto>>
{
    private readonly IAppDbContext _db;

    public GetQuizSessionQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<QuizSessionStateDto>> Handle(GetQuizSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _db.QuizSessions.FirstOrDefaultAsync(
            s => s.Id == request.SessionId && s.UserId == request.UserId,
            cancellationToken);

        if (session is null)
        {
            return Result.Failure<QuizSessionStateDto>("Quiz session not found.", ResultErrorType.NotFound);
        }

        var totalQuestions = await _db.QuizSessionQuestions.CountAsync(sq => sq.SessionId == session.Id, cancellationToken);

        QuizQuestionOptionsDto? currentQuestion = null;
        if (session.Status == QuizSessionStatus.InProgress)
        {
            var currentQuestionEntity = await (
                from sq in _db.QuizSessionQuestions
                join q in _db.QuizQuestions on sq.QuestionId equals q.Id
                where sq.SessionId == session.Id && sq.OrderIndex == session.CurrentQuestionIndex
                select q
            ).FirstOrDefaultAsync(cancellationToken);

            if (currentQuestionEntity is not null)
            {
                currentQuestion = QuizMapping.ToOptionsDto(currentQuestionEntity);
            }
        }

        var result = new QuizSessionStateDto(
            session.Id,
            session.Category,
            session.Difficulty,
            session.Status,
            session.IsDailyChallenge,
            session.CurrentQuestionIndex,
            totalQuestions,
            Math.Max(session.Lives, 0),
            session.ComboCount,
            session.BestComboThisSession,
            session.Score,
            new PowerUpsAvailableDto(FiftyFifty: !session.UsedFiftyFifty, ExtraTime: !session.UsedExtraTime),
            currentQuestion);

        return Result.Success(result);
    }
}
