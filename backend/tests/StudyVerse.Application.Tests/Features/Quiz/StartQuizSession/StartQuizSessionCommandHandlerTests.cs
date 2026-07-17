using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.Quiz.StartQuizSession;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Tests.Features.Quiz.StartQuizSession;

public sealed class StartQuizSessionCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private StartQuizSessionCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

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

    private List<QuizQuestion> SeedQuestions(string category, QuizDifficulty difficulty, int count)
    {
        var questions = Enumerable.Range(0, count)
            .Select(i => new QuizQuestion
            {
                Id = Guid.NewGuid(),
                Category = category,
                Difficulty = difficulty,
                QuestionText = $"Question {i}",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectOptionIndex = 0,
                Explanation = "Because.",
                CreatedAtUtc = _dateTimeProvider.UtcNow,
            })
            .ToList();

        _db.QuizQuestions.AddRange(questions);
        _db.SaveChanges();
        return questions;
    }

    /// <summary>Seeds a Completed session that shows exactly the given questions, for anti-repetition fixtures.</summary>
    private void SeedCompletedSessionShowing(Guid userId, string category, QuizDifficulty difficulty, IReadOnlyList<Guid> questionIds)
    {
        var session = new QuizSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Category = category,
            Difficulty = difficulty,
            Status = QuizSessionStatus.Completed,
            Lives = 3,
            StartedAtUtc = _dateTimeProvider.UtcNow,
            EndedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.QuizSessions.Add(session);

        var order = 0;
        foreach (var questionId in questionIds)
        {
            _db.QuizSessionQuestions.Add(new QuizSessionQuestion
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                QuestionId = questionId,
                OrderIndex = order++,
            });
        }

        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithALargeEnoughFreshPool_ExcludesQuestionsShownInTheLast3CompletedSessions()
    {
        var userId = SeedUser();
        var questions = SeedQuestions(QuizCategories.Science, QuizDifficulty.Easy, 20);

        // 8 questions shown across the user's last 3 completed sessions; 12 remain fresh, which is
        // enough to fill a 10-question session without needing the repeat-allowing fallback.
        var recentlyShownIds = questions.Take(8).Select(q => q.Id).ToList();
        SeedCompletedSessionShowing(userId, QuizCategories.Science, QuizDifficulty.Easy, recentlyShownIds.Take(3).ToList());
        SeedCompletedSessionShowing(userId, QuizCategories.Science, QuizDifficulty.Easy, recentlyShownIds.Skip(3).Take(3).ToList());
        SeedCompletedSessionShowing(userId, QuizCategories.Science, QuizDifficulty.Easy, recentlyShownIds.Skip(6).Take(2).ToList());

        var result = await CreateHandler().Handle(
            new StartQuizSessionCommand(userId, QuizCategories.Science, QuizDifficulty.Easy, false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Questions.Should().HaveCount(10);
        result.Value.Questions.Select(q => q.Id).Should().NotIntersectWith(recentlyShownIds);
    }

    [Fact]
    public async Task Handle_WhenTheFreshPoolIsSmallerThanASessionsWorth_FallsBackToAllowingRepeats()
    {
        var userId = SeedUser();
        // Only 6 questions total for this category+difficulty - fewer than QuestionsPerSession (10).
        var questions = SeedQuestions(QuizCategories.History, QuizDifficulty.Medium, 6);
        SeedCompletedSessionShowing(userId, QuizCategories.History, QuizDifficulty.Medium, questions.Select(q => q.Id).ToList());

        var result = await CreateHandler().Handle(
            new StartQuizSessionCommand(userId, QuizCategories.History, QuizDifficulty.Medium, false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // The fallback tops up with recently-shown questions rather than starting a session short
        // of questions, but the pool itself only has 6 total, so that's the cap here.
        result.Value.Questions.Should().HaveCount(6);
    }

    [Fact]
    public async Task Handle_ForACategoryWithNoSeededQuestions_FailsWithNotFound()
    {
        // Rejecting a category string that isn't one of the 5 known categories is
        // StartQuizSessionCommandValidator's job (enforced by the MediatR ValidationBehavior
        // pipeline, not re-checked in the handler). Calling the handler directly, as these tests
        // do, bypasses that pipeline - so this exercises the handler's own defensive behavior when
        // the question pool for a category+difficulty is empty.
        var userId = SeedUser();

        var result = await CreateHandler().Handle(
            new StartQuizSessionCommand(userId, "Not A Real Category", QuizDifficulty.Easy, false),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_DailyChallengeRequestedForTheWrongCategoryOrDifficulty_FailsValidation()
    {
        var userId = SeedUser();
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var (correctCategory, correctDifficulty) = DailyQuizSelector.GetTodaysChallenge(today);
        var wrongCategory = QuizCategories.All.First(c => c != correctCategory);
        SeedQuestions(wrongCategory, correctDifficulty, 10);

        var result = await CreateHandler().Handle(
            new StartQuizSessionCommand(userId, wrongCategory, correctDifficulty, true),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task Handle_DailyChallengeTwiceInTheSameUtcDay_TheSecondAttemptFailsWithConflict()
    {
        var userId = SeedUser();
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var (category, difficulty) = DailyQuizSelector.GetTodaysChallenge(today);
        SeedQuestions(category, difficulty, 10);

        var first = await CreateHandler().Handle(
            new StartQuizSessionCommand(userId, category, difficulty, true),
            CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var second = await CreateHandler().Handle(
            new StartQuizSessionCommand(userId, category, difficulty, true),
            CancellationToken.None);

        second.IsSuccess.Should().BeFalse();
        second.ErrorType.Should().Be(ResultErrorType.Conflict);

        (await _db.QuizSessions.CountAsync(s => s.UserId == userId)).Should().Be(1);
    }

    [Fact]
    public async Task Handle_DailyChallengeOnANewUtcDay_CanBePlayedAgain()
    {
        var userId = SeedUser();
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var (category, difficulty) = DailyQuizSelector.GetTodaysChallenge(today);
        SeedQuestions(category, difficulty, 10);

        var first = await CreateHandler().Handle(
            new StartQuizSessionCommand(userId, category, difficulty, true),
            CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(1);
        var tomorrow = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var (tomorrowCategory, tomorrowDifficulty) = DailyQuizSelector.GetTodaysChallenge(tomorrow);
        SeedQuestions(tomorrowCategory, tomorrowDifficulty, 10);

        var second = await CreateHandler().Handle(
            new StartQuizSessionCommand(userId, tomorrowCategory, tomorrowDifficulty, true),
            CancellationToken.None);

        second.IsSuccess.Should().BeTrue();
    }
}
