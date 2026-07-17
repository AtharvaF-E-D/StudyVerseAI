using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.Quiz.SubmitAnswer;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Tests.Features.Quiz.SubmitAnswer;

public sealed class SubmitAnswerCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private SubmitAnswerCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

    private Guid SeedUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            DisplayName = "Student",
            AuthProvider = AuthProvider.Local,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Users.Add(user);
        _db.SaveChanges();
        return user.Id;
    }

    /// <summary>Seeds an InProgress session with <paramref name="questionCount"/> questions, each with correct option index 0.</summary>
    private (QuizSession Session, List<QuizQuestion> Questions) SeedInProgressSession(
        Guid userId,
        int questionCount,
        int lives = 3,
        bool isDailyChallenge = false)
    {
        var questions = Enumerable.Range(0, questionCount)
            .Select(i => new QuizQuestion
            {
                Id = Guid.NewGuid(),
                Category = QuizCategories.Science,
                Difficulty = QuizDifficulty.Easy,
                QuestionText = $"Question {i}",
                OptionA = "Correct",
                OptionB = "Wrong 1",
                OptionC = "Wrong 2",
                OptionD = "Wrong 3",
                CorrectOptionIndex = 0,
                Explanation = "Because.",
                CreatedAtUtc = _dateTimeProvider.UtcNow,
            })
            .ToList();
        _db.QuizQuestions.AddRange(questions);

        var session = new QuizSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Category = QuizCategories.Science,
            Difficulty = QuizDifficulty.Easy,
            Status = QuizSessionStatus.InProgress,
            Lives = lives,
            CurrentQuestionIndex = 0,
            IsDailyChallenge = isDailyChallenge,
            DailyChallengeDateUtc = isDailyChallenge ? DateOnly.FromDateTime(_dateTimeProvider.UtcNow) : null,
            StartedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.QuizSessions.Add(session);

        for (var i = 0; i < questions.Count; i++)
        {
            _db.QuizSessionQuestions.Add(new QuizSessionQuestion
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                QuestionId = questions[i].Id,
                OrderIndex = i,
            });
        }

        _db.SaveChanges();
        return (session, questions);
    }

    [Fact]
    public async Task Handle_AllQuestionsAnsweredCorrectly_CompletesTheSessionAndAwardsProgressExactlyOnce()
    {
        var userId = SeedUser();
        var (session, questions) = SeedInProgressSession(userId, questionCount: 2);
        var handler = CreateHandler();

        var first = await handler.Handle(new SubmitAnswerCommand(userId, session.Id, questions[0].Id, 0, 1000), CancellationToken.None);
        first.Value.IsCorrect.Should().BeTrue();
        first.Value.IsSessionComplete.Should().BeFalse();

        var second = await handler.Handle(new SubmitAnswerCommand(userId, session.Id, questions[1].Id, 0, 1000), CancellationToken.None);

        second.Value.IsSessionComplete.Should().BeTrue();
        second.Value.SessionSummary.Should().NotBeNull();
        second.Value.SessionSummary!.CompletedAllQuestions.Should().BeTrue();
        second.Value.SessionSummary.RanOutOfLives.Should().BeFalse();

        var updatedSession = await _db.QuizSessions.SingleAsync(s => s.Id == session.Id);
        updatedSession.Status.Should().Be(QuizSessionStatus.Completed);

        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.Xp.Should().Be(updatedSession.XpEarned);
        progress.Coins.Should().Be(updatedSession.CoinsEarned);
    }

    [Fact]
    public async Task Handle_LivesReachingZero_CompletesTheSessionEvenWithQuestionsRemaining()
    {
        var userId = SeedUser();
        var (session, questions) = SeedInProgressSession(userId, questionCount: 10, lives: 3);
        var handler = CreateHandler();

        var first = await handler.Handle(new SubmitAnswerCommand(userId, session.Id, questions[0].Id, 1, null), CancellationToken.None);
        first.Value.IsCorrect.Should().BeFalse();
        first.Value.LivesRemaining.Should().Be(2);
        first.Value.IsSessionComplete.Should().BeFalse();

        var second = await handler.Handle(new SubmitAnswerCommand(userId, session.Id, questions[1].Id, 1, null), CancellationToken.None);
        second.Value.LivesRemaining.Should().Be(1);
        second.Value.IsSessionComplete.Should().BeFalse();

        var third = await handler.Handle(new SubmitAnswerCommand(userId, session.Id, questions[2].Id, 1, null), CancellationToken.None);

        third.Value.LivesRemaining.Should().Be(0);
        third.Value.IsSessionComplete.Should().BeTrue();
        third.Value.SessionSummary!.RanOutOfLives.Should().BeTrue();
        third.Value.SessionSummary.CompletedAllQuestions.Should().BeFalse();

        var updatedSession = await _db.QuizSessions.SingleAsync(s => s.Id == session.Id);
        updatedSession.Status.Should().Be(QuizSessionStatus.Completed);
        // Only 3 of the 10 questions were ever answered - the session ended early on lives, not on question count.
        updatedSession.CurrentQuestionIndex.Should().Be(3);
    }

    [Fact]
    public async Task Handle_AWrongAnswerAfterCorrectOnes_ResetsTheComboToZero()
    {
        var userId = SeedUser();
        var (session, questions) = SeedInProgressSession(userId, questionCount: 3);
        var handler = CreateHandler();

        await handler.Handle(new SubmitAnswerCommand(userId, session.Id, questions[0].Id, 0, null), CancellationToken.None);
        var afterWrong = await handler.Handle(new SubmitAnswerCommand(userId, session.Id, questions[1].Id, 1, null), CancellationToken.None);

        afterWrong.Value.ComboCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ForASessionThatIsNotInProgress_FailsWithConflict()
    {
        var userId = SeedUser();
        var (session, questions) = SeedInProgressSession(userId, questionCount: 1);
        session.Status = QuizSessionStatus.Abandoned;
        await _db.SaveChangesAsync();

        var result = await CreateHandler().Handle(
            new SubmitAnswerCommand(userId, session.Id, questions[0].Id, 0, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_ForASessionOwnedByAnotherUser_FailsWithNotFound()
    {
        var ownerId = SeedUser();
        var otherUserId = SeedUser();
        var (session, questions) = SeedInProgressSession(ownerId, questionCount: 1);

        var result = await CreateHandler().Handle(
            new SubmitAnswerCommand(otherUserId, session.Id, questions[0].Id, 0, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_OutOfOrderQuestionSubmission_FailsValidationAndDoesNotMutateSessionState()
    {
        var userId = SeedUser();
        var (session, questions) = SeedInProgressSession(userId, questionCount: 3);

        // CurrentQuestionIndex is 0 (questions[0]) - submitting for questions[1] is out of order.
        var result = await CreateHandler().Handle(
            new SubmitAnswerCommand(userId, session.Id, questions[1].Id, 0, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);

        var unchangedSession = await _db.QuizSessions.SingleAsync(s => s.Id == session.Id);
        unchangedSession.CurrentQuestionIndex.Should().Be(0);
    }

    [Fact]
    public async Task Handle_TheFifthConsecutiveCorrectAnswer_AppliesTheFullComboMultiplier()
    {
        var userId = SeedUser();
        var (session, questions) = SeedInProgressSession(userId, questionCount: 10);
        var handler = CreateHandler();

        for (var i = 0; i < 4; i++)
        {
            await handler.Handle(new SubmitAnswerCommand(userId, session.Id, questions[i].Id, 0, null), CancellationToken.None);
        }

        var fifth = await handler.Handle(new SubmitAnswerCommand(userId, session.Id, questions[4].Id, 0, null), CancellationToken.None);

        fifth.Value.ComboCount.Should().Be(5);
        // Easy base XP (10) x 1.5x combo multiplier = 15.
        fifth.Value.XpEarnedThisAnswer.Should().Be(15);
    }

    [Fact]
    public async Task Handle_ADailyChallengeSessionThatCompletes_AwardsTheBonusOnlyAtCompletion()
    {
        var userId = SeedUser();
        var (session, questions) = SeedInProgressSession(userId, questionCount: 1, isDailyChallenge: true);

        var result = await CreateHandler().Handle(
            new SubmitAnswerCommand(userId, session.Id, questions[0].Id, 0, null),
            CancellationToken.None);

        result.Value.IsSessionComplete.Should().BeTrue();
        result.Value.SessionSummary!.DailyChallengeBonusXp.Should().Be(QuizScoring.DailyChallengeBonusXp);
        result.Value.SessionSummary.DailyChallengeBonusCoins.Should().Be(QuizScoring.DailyChallengeBonusCoins);

        var updatedSession = await _db.QuizSessions.SingleAsync(s => s.Id == session.Id);
        updatedSession.XpEarned.Should().Be(updatedSession.Score + QuizScoring.DailyChallengeBonusXp);
    }
}
