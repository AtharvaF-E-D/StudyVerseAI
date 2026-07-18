using FluentAssertions;
using StudyVerse.Application.Features.MockTests.GetMockTestReview;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.MockTests;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Tests.Features.MockTests.GetMockTestReview;

public sealed class GetMockTestReviewQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetMockTestReviewQueryHandler CreateHandler() => new(_db);

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

    private (MockTestAttempt Attempt, QuizQuestion Question) SeedSubmittedAttemptWithOneQuestion(Guid userId, int? selectedOptionIndex, bool isCorrect)
    {
        var question = new QuizQuestion
        {
            Id = Guid.NewGuid(),
            Category = QuizCategories.Science,
            Difficulty = QuizDifficulty.Easy,
            QuestionText = "What is H2O?",
            OptionA = "Water",
            OptionB = "Salt",
            OptionC = "Sugar",
            OptionD = "Oil",
            CorrectOptionIndex = 0,
            Explanation = "H2O is the chemical formula for water.",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.QuizQuestions.Add(question);

        var attempt = new MockTestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateId = MockTestCatalog.All[0].Id,
            Status = MockTestAttemptStatus.Submitted,
            StartedAtUtc = _dateTimeProvider.UtcNow,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
            Score = isCorrect ? 100 : 0,
            CorrectCount = isCorrect ? 1 : 0,
            TotalQuestions = 1,
            PercentileRank = 100,
            AiWeaknessAnalysis = "Some analysis.",
        };
        _db.MockTestAttempts.Add(attempt);

        _db.MockTestAttemptAnswers.Add(new MockTestAttemptAnswer
        {
            Id = Guid.NewGuid(),
            AttemptId = attempt.Id,
            QuestionId = question.Id,
            OrderIndex = 0,
            SelectedOptionIndex = selectedOptionIndex,
            IsCorrect = isCorrect,
        });

        _db.SaveChanges();
        return (attempt, question);
    }

    [Fact]
    public async Task Handle_ForASubmittedAttempt_ReturnsEveryQuestionWithAnswerAndExplanation()
    {
        var userId = SeedUser();
        var (attempt, question) = SeedSubmittedAttemptWithOneQuestion(userId, selectedOptionIndex: 1, isCorrect: false);

        var result = await CreateHandler().Handle(new GetMockTestReviewQuery(userId, attempt.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Questions.Should().HaveCount(1);
        var reviewQuestion = result.Value.Questions[0];
        reviewQuestion.QuestionId.Should().Be(question.Id);
        reviewQuestion.SelectedOptionIndex.Should().Be(1);
        reviewQuestion.CorrectOptionIndex.Should().Be(0);
        reviewQuestion.IsCorrect.Should().BeFalse();
        reviewQuestion.Explanation.Should().Be(question.Explanation);
        reviewQuestion.Options.Should().Equal("Water", "Salt", "Sugar", "Oil");
    }

    [Fact]
    public async Task Handle_ForAnInProgressAttempt_FailsWithValidation()
    {
        var userId = SeedUser();
        var attempt = new MockTestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateId = MockTestCatalog.All[0].Id,
            Status = MockTestAttemptStatus.InProgress,
            StartedAtUtc = _dateTimeProvider.UtcNow,
            TotalQuestions = 5,
        };
        _db.MockTestAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        var result = await CreateHandler().Handle(new GetMockTestReviewQuery(userId, attempt.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ForAnAttemptOwnedByAnotherUser_FailsWithNotFound()
    {
        var ownerId = SeedUser();
        var otherUserId = SeedUser();
        var (attempt, _) = SeedSubmittedAttemptWithOneQuestion(ownerId, selectedOptionIndex: 0, isCorrect: true);

        var result = await CreateHandler().Handle(new GetMockTestReviewQuery(otherUserId, attempt.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
