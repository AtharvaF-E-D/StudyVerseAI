using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.InterviewPrep.StartInterviewSession;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.InterviewPrep.StartInterviewSession;

public sealed class StartInterviewSessionCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private StartInterviewSessionCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

    private void SeedQuestions(InterviewQuestionType type, int count)
    {
        for (var i = 0; i < count; i++)
        {
            _db.InterviewQuestions.Add(new InterviewQuestion
            {
                Id = Guid.NewGuid(),
                Type = type,
                QuestionText = $"{type} question {i}",
                WhatGoodAnswersCover = "Anything relevant to the question.",
                CreatedAtUtc = _dateTimeProvider.UtcNow,
            });
        }

        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_ValidType_SelectsExactlyFiveQuestionsOfThatTypeOnly()
    {
        SeedQuestions(InterviewQuestionType.Hr, 12);
        SeedQuestions(InterviewQuestionType.Technical, 12);
        var userId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new StartInterviewSessionCommand(userId, InterviewQuestionType.Hr), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(InterviewQuestionType.Hr);
        result.Value.Status.Should().Be(InterviewSessionStatus.InProgress);
        result.Value.Questions.Should().HaveCount(5);
        result.Value.Questions.Select(q => q.QuestionText).Should().OnlyContain(t => t.StartsWith("Hr question"));
        // No duplicates within the selection.
        result.Value.Questions.Select(q => q.QuestionId).Should().OnlyHaveUniqueItems();

        var persisted = await _db.InterviewSessions.SingleAsync(s => s.UserId == userId);
        persisted.Type.Should().Be(InterviewQuestionType.Hr);
        persisted.Status.Should().Be(InterviewSessionStatus.InProgress);
    }

    [Fact]
    public async Task Handle_NoQuestionsSeededForRequestedType_ReturnsNotFound()
    {
        SeedQuestions(InterviewQuestionType.Hr, 12);

        var result = await CreateHandler().Handle(
            new StartInterviewSessionCommand(Guid.NewGuid(), InterviewQuestionType.Technical),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_FewerThanFiveQuestionsAvailable_SelectsWhateverExists()
    {
        SeedQuestions(InterviewQuestionType.Behavioral, 3);

        var result = await CreateHandler().Handle(
            new StartInterviewSessionCommand(Guid.NewGuid(), InterviewQuestionType.Behavioral),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Questions.Should().HaveCount(3);
    }
}
