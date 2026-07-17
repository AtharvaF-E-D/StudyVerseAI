using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Quiz.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Features.Quiz.StartQuizSession;

public sealed class StartQuizSessionCommandHandler : IRequestHandler<StartQuizSessionCommand, Result<StartQuizSessionResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StartQuizSessionCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<StartQuizSessionResultDto>> Handle(
        StartQuizSessionCommand request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        if (request.IsDailyChallenge)
        {
            var (dailyCategory, dailyDifficulty) = DailyQuizSelector.GetTodaysChallenge(today);
            if (request.Category != dailyCategory || request.Difficulty != dailyDifficulty)
            {
                return Result.Failure<StartQuizSessionResultDto>(
                    $"Today's daily challenge is {dailyCategory} ({dailyDifficulty}), not the requested category/difficulty.",
                    ResultErrorType.Validation);
            }

            // Pre-check for a friendly error message; the unique index on
            // (UserId, DailyChallengeDateUtc) is the actual source of truth guarding against a
            // concurrent second daily-challenge attempt the same day.
            var alreadyAttemptedToday = await _db.QuizSessions.AnyAsync(
                s => s.UserId == request.UserId && s.DailyChallengeDateUtc == today,
                cancellationToken);

            if (alreadyAttemptedToday)
            {
                return Result.Failure<StartQuizSessionResultDto>(
                    "You've already played today's daily quiz challenge. Come back tomorrow!",
                    ResultErrorType.Conflict);
            }
        }

        var questions = await QuizQuestionSelectionService.SelectQuestionsAsync(
            _db, request.UserId, request.Category, request.Difficulty, cancellationToken);

        if (questions.Count == 0)
        {
            return Result.Failure<StartQuizSessionResultDto>(
                "No questions are available for that category and difficulty yet.",
                ResultErrorType.NotFound);
        }

        var now = _dateTimeProvider.UtcNow;
        var session = new QuizSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Category = request.Category,
            Difficulty = request.Difficulty,
            Status = QuizSessionStatus.InProgress,
            Lives = QuizScoring.StartingLives,
            CurrentQuestionIndex = 0,
            ComboCount = 0,
            BestComboThisSession = 0,
            Score = 0,
            XpEarned = 0,
            CoinsEarned = 0,
            UsedFiftyFifty = false,
            UsedExtraTime = false,
            IsDailyChallenge = request.IsDailyChallenge,
            DailyChallengeDateUtc = request.IsDailyChallenge ? today : null,
            StartedAtUtc = now,
        };
        _db.QuizSessions.Add(session);

        for (var index = 0; index < questions.Count; index++)
        {
            _db.QuizSessionQuestions.Add(new QuizSessionQuestion
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                QuestionId = questions[index].Id,
                OrderIndex = index,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        var result = new StartQuizSessionResultDto(
            session.Id,
            questions.Select(QuizMapping.ToOptionsDto).ToList(),
            session.Lives,
            new PowerUpsAvailableDto(FiftyFifty: !session.UsedFiftyFifty, ExtraTime: !session.UsedExtraTime),
            questions.Count);

        return Result.Success(result);
    }
}
