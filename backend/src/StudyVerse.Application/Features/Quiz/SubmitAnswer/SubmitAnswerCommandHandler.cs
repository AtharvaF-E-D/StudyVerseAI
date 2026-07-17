using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Features.Quiz.SubmitAnswer;

/// <summary>
/// The core Rapid Fire Quiz loop: validates ownership/state/turn-order, looks up the real correct
/// answer server-side (the client only ever sees question text + options — see
/// <c>StartQuizSessionCommandHandler</c>/<c>GetQuizSessionQueryHandler</c>), scores the answer,
/// and — if this answer ends the session (lives hit 0 or all questions answered) — credits the
/// session's accumulated XP/coins to <see cref="UserProgress"/> exactly once, all in a single
/// <c>SaveChangesAsync</c>. Mirrors the "read-validate-mutate-persist in one save" shape of
/// <c>SendMessageCommandHandler</c> and <c>CompleteChallengeCommandHandler</c>.
/// </summary>
public sealed class SubmitAnswerCommandHandler : IRequestHandler<SubmitAnswerCommand, Result<SubmitAnswerResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SubmitAnswerCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SubmitAnswerResultDto>> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.QuizSessions.FirstOrDefaultAsync(
            s => s.Id == request.SessionId && s.UserId == request.UserId,
            cancellationToken);

        if (session is null)
        {
            return Result.Failure<SubmitAnswerResultDto>("Quiz session not found.", ResultErrorType.NotFound);
        }

        if (session.Status != QuizSessionStatus.InProgress)
        {
            return Result.Failure<SubmitAnswerResultDto>(
                "This quiz session is no longer in progress.",
                ResultErrorType.Conflict);
        }

        var sessionQuestions = await _db.QuizSessionQuestions
            .Where(sq => sq.SessionId == session.Id)
            .OrderBy(sq => sq.OrderIndex)
            .ToListAsync(cancellationToken);

        var totalQuestions = sessionQuestions.Count;

        var currentSessionQuestion = sessionQuestions.ElementAtOrDefault(session.CurrentQuestionIndex);
        if (currentSessionQuestion is null)
        {
            // Defensive: the session should already have been marked Completed once
            // CurrentQuestionIndex reached the end. Reaching here means state is inconsistent.
            return Result.Failure<SubmitAnswerResultDto>(
                "This quiz session has no more questions to answer.",
                ResultErrorType.Conflict);
        }

        if (currentSessionQuestion.QuestionId != request.QuestionId)
        {
            return Result.Failure<SubmitAnswerResultDto>(
                "That is not the current question for this session — answers must be submitted in order.",
                ResultErrorType.Validation);
        }

        var question = await _db.QuizQuestions.FirstOrDefaultAsync(
            q => q.Id == request.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure<SubmitAnswerResultDto>("Question not found.", ResultErrorType.NotFound);
        }

        var now = _dateTimeProvider.UtcNow;
        var isCorrect = request.SelectedOptionIndex == question.CorrectOptionIndex;

        currentSessionQuestion.SelectedOptionIndex = request.SelectedOptionIndex;
        currentSessionQuestion.IsCorrect = isCorrect;
        currentSessionQuestion.TimeTakenMs = request.TimeTakenMs;
        currentSessionQuestion.AnsweredAtUtc = now;

        var xpEarnedThisAnswer = 0;

        if (isCorrect)
        {
            session.ComboCount++;
            session.BestComboThisSession = Math.Max(session.BestComboThisSession, session.ComboCount);

            xpEarnedThisAnswer = QuizScoring.GetXpForCorrectAnswer(session.Difficulty, session.ComboCount);
            var coinsEarnedThisAnswer = QuizScoring.GetCoinReward(session.Difficulty);

            session.Score += xpEarnedThisAnswer;
            session.XpEarned += xpEarnedThisAnswer;
            session.CoinsEarned += coinsEarnedThisAnswer;
        }
        else
        {
            session.Lives--;
            session.ComboCount = 0;
        }

        session.CurrentQuestionIndex++;

        var completedAllQuestions = session.CurrentQuestionIndex >= totalQuestions;
        var ranOutOfLives = session.Lives <= 0;
        var isSessionComplete = completedAllQuestions || ranOutOfLives;

        QuizSessionSummaryDto? summary = null;

        if (isSessionComplete)
        {
            session.Status = QuizSessionStatus.Completed;
            session.EndedAtUtc = now;

            var dailyBonusXp = 0;
            var dailyBonusCoins = 0;
            if (session.IsDailyChallenge)
            {
                dailyBonusXp = QuizScoring.DailyChallengeBonusXp;
                dailyBonusCoins = QuizScoring.DailyChallengeBonusCoins;
                session.XpEarned += dailyBonusXp;
                session.CoinsEarned += dailyBonusCoins;
            }

            // Credited exactly once, here at completion — never per-question — so resuming or
            // retrying a session can never double-award XP/coins.
            var progress = await _db.UserProgresses.FirstOrDefaultAsync(
                p => p.UserId == request.UserId,
                cancellationToken);
            if (progress is null)
            {
                progress = new UserProgress { UserId = request.UserId };
                _db.UserProgresses.Add(progress);
            }

            progress.Xp += session.XpEarned;
            progress.Coins += session.CoinsEarned;

            var correctAnswers = sessionQuestions.Count(sq => sq.IsCorrect == true);

            summary = new QuizSessionSummaryDto(
                totalQuestions,
                correctAnswers,
                session.Score,
                session.XpEarned,
                session.CoinsEarned,
                session.BestComboThisSession,
                completedAllQuestions,
                ranOutOfLives,
                dailyBonusXp,
                dailyBonusCoins);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var result = new SubmitAnswerResultDto(
            isCorrect,
            question.CorrectOptionIndex,
            question.Explanation,
            xpEarnedThisAnswer,
            session.ComboCount,
            Math.Max(session.Lives, 0),
            isSessionComplete,
            summary);

        return Result.Success(result);
    }
}
