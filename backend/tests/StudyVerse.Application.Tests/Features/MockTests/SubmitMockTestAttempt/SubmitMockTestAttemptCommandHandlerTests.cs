using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.MockTests.SubmitMockTestAttempt;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.MockTests;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Tests.Features.MockTests.SubmitMockTestAttempt;

public sealed class SubmitMockTestAttemptCommandHandlerTests
{
    private static readonly Guid TemplateId = MockTestCatalog.All[0].Id;

    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();
    private readonly IAiChatProvider _aiChatProvider = Substitute.For<IAiChatProvider>();

    private SubmitMockTestAttemptCommandHandler CreateHandler() => new(_db, _dateTimeProvider, _aiChatProvider);

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

    /// <summary>Seeds an InProgress attempt with <paramref name="questionCount"/> real questions, each with correct option index 0.</summary>
    private (MockTestAttempt Attempt, List<QuizQuestion> Questions) SeedInProgressAttempt(Guid userId, int questionCount, Guid? templateId = null)
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

        var attempt = new MockTestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateId = templateId ?? TemplateId,
            Status = MockTestAttemptStatus.InProgress,
            StartedAtUtc = _dateTimeProvider.UtcNow,
            TotalQuestions = questionCount,
        };
        _db.MockTestAttempts.Add(attempt);

        for (var i = 0; i < questions.Count; i++)
        {
            _db.MockTestAttemptAnswers.Add(new MockTestAttemptAnswer
            {
                Id = Guid.NewGuid(),
                AttemptId = attempt.Id,
                QuestionId = questions[i].Id,
                OrderIndex = i,
            });
        }

        _db.SaveChanges();
        return (attempt, questions);
    }

    /// <summary>Seeds an already-Submitted attempt for the same template, with only the Score column populated - the percentile query only reads Score.</summary>
    private void SeedOtherSubmittedAttempt(int score, Guid? templateId = null)
    {
        _db.MockTestAttempts.Add(new MockTestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TemplateId = templateId ?? TemplateId,
            Status = MockTestAttemptStatus.Submitted,
            StartedAtUtc = _dateTimeProvider.UtcNow,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
            Score = score,
            CorrectCount = 0,
            TotalQuestions = 1,
            PercentileRank = 0,
        });
        _db.SaveChanges();
    }

    private void StubAiCompletion(string content) =>
        _aiChatProvider
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new AiChatResult(content, 100, 50));

    [Fact]
    public async Task Handle_MixOfCorrectWrongAndUnansweredQuestions_ScoresUnansweredAsWrong()
    {
        var userId = SeedUser();
        var (attempt, questions) = SeedInProgressAttempt(userId, questionCount: 4);
        StubAiCompletion("Focus on Science fundamentals.");

        // q0 correct, q1 wrong, q2 left unanswered entirely (not in the Answers list), q3 correct.
        var answers = new List<MockTestAnswerInput>
        {
            new(questions[0].Id, 0),
            new(questions[1].Id, 1),
            new(questions[3].Id, 0),
        };

        var result = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(userId, attempt.Id, answers),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CorrectCount.Should().Be(2);
        result.Value.TotalQuestions.Should().Be(4);
        result.Value.Score.Should().Be(50); // 2/4 = 50%

        var unansweredRow = await _db.MockTestAttemptAnswers.SingleAsync(a => a.QuestionId == questions[2].Id);
        unansweredRow.SelectedOptionIndex.Should().BeNull();
        unansweredRow.IsCorrect.Should().BeFalse();

        var updatedAttempt = await _db.MockTestAttempts.SingleAsync(a => a.Id == attempt.Id);
        updatedAttempt.Status.Should().Be(MockTestAttemptStatus.Submitted);
        updatedAttempt.SubmittedAtUtc.Should().NotBeNull();
        updatedAttempt.AiWeaknessAnalysis.Should().Be("Focus on Science fundamentals.");
    }

    [Fact]
    public async Task Handle_ZeroOtherSubmittedAttemptsForTheTemplate_PercentileRankIs100()
    {
        var userId = SeedUser();
        var (attempt, questions) = SeedInProgressAttempt(userId, questionCount: 2);
        StubAiCompletion("Some analysis.");

        var result = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(userId, attempt.Id, [new MockTestAnswerInput(questions[0].Id, 1)]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PercentileRank.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WithAKnownPriorScoreDistribution_ComputesTheStandardMeanRankPercentile()
    {
        var userId = SeedUser();
        // 5 questions so scores land on clean 20-point increments (0/20/40/60/80/100).
        var (attempt, questions) = SeedInProgressAttempt(userId, questionCount: 5);
        StubAiCompletion("Some analysis.");

        SeedOtherSubmittedAttempt(score: 40);
        SeedOtherSubmittedAttempt(score: 60);
        SeedOtherSubmittedAttempt(score: 60);
        SeedOtherSubmittedAttempt(score: 80);

        // Answer exactly 3 of 5 correctly -> 60% score, tying two of the four other attempts.
        var answers = questions.Take(3).Select(q => new MockTestAnswerInput(q.Id, 0)).ToList();

        var result = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(userId, attempt.Id, answers),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Score.Should().Be(60);
        // strictlyLower=1 (the 40), equal=2 (the two 60s), total=4 -> (1 + 0.5*2)/4*100 = 50.
        result.Value.PercentileRank.Should().Be(50);
    }

    [Fact]
    public async Task Handle_TiedForFirstWithEveryOtherSubmittedAttempt_LandsAtTheFiftiethPercentileNotTheHundredth()
    {
        var userId = SeedUser();
        var (attempt, questions) = SeedInProgressAttempt(userId, questionCount: 2);
        StubAiCompletion("Some analysis.");

        SeedOtherSubmittedAttempt(score: 100);
        SeedOtherSubmittedAttempt(score: 100);

        // Both questions correct -> 100% score, tying both other attempts exactly.
        var answers = questions.Select(q => new MockTestAnswerInput(q.Id, 0)).ToList();

        var result = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(userId, attempt.Id, answers),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Score.Should().Be(100);
        // strictlyLower=0, equal=2, total=2 -> (0 + 0.5*2)/2*100 = 50.
        result.Value.PercentileRank.Should().Be(50);
    }

    [Fact]
    public async Task Handle_AHigherScoringSubmissionRanksHigherThanALowerScoringOneAgainstTheSameField()
    {
        var userId = SeedUser();
        SeedOtherSubmittedAttempt(score: 30);
        SeedOtherSubmittedAttempt(score: 50);
        SeedOtherSubmittedAttempt(score: 70);
        StubAiCompletion("Some analysis.");

        var (lowAttempt, lowQuestions) = SeedInProgressAttempt(userId, questionCount: 5);
        var lowResult = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(userId, lowAttempt.Id, [new MockTestAnswerInput(lowQuestions[0].Id, 0)]), // 1/5 = 20%
            CancellationToken.None);

        var (highAttempt, highQuestions) = SeedInProgressAttempt(userId, questionCount: 5);
        var highResult = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(
                userId,
                highAttempt.Id,
                highQuestions.Select(q => new MockTestAnswerInput(q.Id, 0)).ToList()), // 5/5 = 100%
            CancellationToken.None);

        highResult.Value.PercentileRank.Should().BeGreaterThan(lowResult.Value.PercentileRank);
    }

    [Fact]
    public async Task Handle_APerfectScore_SkipsTheAiCallAndUsesACannedCongratulatoryMessage()
    {
        var userId = SeedUser();
        var (attempt, questions) = SeedInProgressAttempt(userId, questionCount: 2);

        var answers = questions.Select(q => new MockTestAnswerInput(q.Id, 0)).ToList();

        var result = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(userId, attempt.Id, answers),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AiWeaknessAnalysis.Should().NotBeNullOrWhiteSpace();
        await _aiChatProvider.DidNotReceiveWithAnyArgs().GetCompletionAsync(default!, default);
    }

    [Fact]
    public async Task Handle_ForAnAttemptOwnedByAnotherUser_FailsWithNotFound()
    {
        var ownerId = SeedUser();
        var otherUserId = SeedUser();
        var (attempt, questions) = SeedInProgressAttempt(ownerId, questionCount: 1);

        var result = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(otherUserId, attempt.Id, [new MockTestAnswerInput(questions[0].Id, 0)]),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        await _aiChatProvider.DidNotReceiveWithAnyArgs().GetCompletionAsync(default!, default);
    }

    [Fact]
    public async Task Handle_AnAttemptThatWasAlreadySubmitted_FailsWithConflictAndDoesNotResubmit()
    {
        var userId = SeedUser();
        var (attempt, questions) = SeedInProgressAttempt(userId, questionCount: 1);
        StubAiCompletion("First analysis.");

        var first = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(userId, attempt.Id, [new MockTestAnswerInput(questions[0].Id, 0)]),
            CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var second = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(userId, attempt.Id, [new MockTestAnswerInput(questions[0].Id, 0)]),
            CancellationToken.None);

        second.IsSuccess.Should().BeFalse();
        second.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_ForAnAttemptThatDoesNotExist_FailsWithNotFound()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(
            new SubmitMockTestAttemptCommand(userId, Guid.NewGuid(), []),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
