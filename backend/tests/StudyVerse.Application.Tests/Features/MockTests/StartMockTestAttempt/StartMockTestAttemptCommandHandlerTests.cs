using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.MockTests.StartMockTestAttempt;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.MockTests;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Tests.Features.MockTests.StartMockTestAttempt;

public sealed class StartMockTestAttemptCommandHandlerTests
{
    private static readonly MockTestTemplate ScienceTemplate =
        MockTestCatalog.All.Single(t => t.Category == QuizCategories.Science);

    private static readonly MockTestTemplate MixedTemplate =
        MockTestCatalog.All.Single(t => t.Category == MockTestCatalog.MixedCategory);

    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private StartMockTestAttemptCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

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

    private List<QuizQuestion> SeedQuestions(string category, int count)
    {
        var questions = Enumerable.Range(0, count)
            .Select(i => new QuizQuestion
            {
                Id = Guid.NewGuid(),
                Category = category,
                Difficulty = QuizDifficulty.Easy,
                QuestionText = $"{category} question {i}",
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

    [Fact]
    public async Task Handle_ForASingleCategoryTemplate_PullsExactlyTheTemplatesQuestionCountFromThatCategory()
    {
        var userId = SeedUser();
        // More than enough Science questions available, plus a decoy category that must never be pulled.
        var scienceQuestions = SeedQuestions(QuizCategories.Science, 18);
        SeedQuestions(QuizCategories.Mathematics, 18);

        var result = await CreateHandler().Handle(
            new StartMockTestAttemptCommand(userId, ScienceTemplate.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Questions.Should().HaveCount(ScienceTemplate.QuestionCount);
        result.Value.Questions.Select(q => q.Id).Should().BeSubsetOf(scienceQuestions.Select(q => q.Id));
        result.Value.DurationMinutes.Should().Be(ScienceTemplate.DurationMinutes);

        // Never leaks the correct answer in the started-attempt projection.
        result.Value.Questions.Should().OnlyContain(q => q.Options.Count == 4);

        var attempt = await _db.MockTestAttempts.SingleAsync(a => a.UserId == userId);
        attempt.TemplateId.Should().Be(ScienceTemplate.Id);
        attempt.Status.Should().Be(MockTestAttemptStatus.InProgress);
        attempt.TotalQuestions.Should().Be(ScienceTemplate.QuestionCount);

        var answerRows = await _db.MockTestAttemptAnswers.Where(a => a.AttemptId == attempt.Id).ToListAsync();
        answerRows.Should().HaveCount(ScienceTemplate.QuestionCount);
        answerRows.Select(a => a.QuestionId).Should().BeSubsetOf(scienceQuestions.Select(q => q.Id));
        answerRows.Should().OnlyContain(a => a.SelectedOptionIndex == null && a.IsCorrect == false);
    }

    [Fact]
    public async Task Handle_ForTheMixedCategoryTemplate_PullsFromAllFiveCategoriesNotJustOne()
    {
        var userId = SeedUser();
        foreach (var category in QuizCategories.All)
        {
            SeedQuestions(category, 6);
        }

        var result = await CreateHandler().Handle(
            new StartMockTestAttemptCommand(userId, MixedTemplate.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Questions.Should().HaveCount(MixedTemplate.QuestionCount);

        var selectedQuestionIds = result.Value.Questions.Select(q => q.Id).ToList();
        var selectedCategories = await _db.QuizQuestions
            .Where(q => selectedQuestionIds.Contains(q.Id))
            .Select(q => q.Category)
            .Distinct()
            .ToListAsync();

        // With 30 questions total spread evenly across 5 categories and 20 drawn at random, it would
        // take an astronomically unlucky shuffle to land entirely within one category - a healthy
        // spread here is strong evidence the Mixed pool wasn't filtered down to a single category.
        selectedCategories.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task Handle_ForAnUnknownTemplateId_FailsWithNotFound()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(
            new StartMockTestAttemptCommand(userId, Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNoQuestionsExistForTheTemplatesCategory_FailsWithNotFound()
    {
        var userId = SeedUser();
        // No questions seeded at all.

        var result = await CreateHandler().Handle(
            new StartMockTestAttemptCommand(userId, ScienceTemplate.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenFewerQuestionsExistThanRequested_ReturnsWhateverIsAvailableRatherThanFailing()
    {
        var userId = SeedUser();
        // Only 5 Science questions exist, fewer than the template's QuestionCount.
        SeedQuestions(QuizCategories.Science, 5);

        var result = await CreateHandler().Handle(
            new StartMockTestAttemptCommand(userId, ScienceTemplate.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Questions.Should().HaveCount(5);
    }
}
