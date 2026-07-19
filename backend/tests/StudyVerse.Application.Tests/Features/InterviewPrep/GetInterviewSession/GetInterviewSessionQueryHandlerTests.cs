using System.Text.Json;
using FluentAssertions;
using StudyVerse.Application.Features.InterviewPrep.GetInterviewSession;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.InterviewPrep.GetInterviewSession;

public sealed class GetInterviewSessionQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetInterviewSessionQueryHandler CreateHandler() => new(_db);

    private (InterviewSession Session, Guid QuestionId) SeedSessionWithOneAnsweredQuestion(Guid userId)
    {
        var question = new InterviewQuestion
        {
            Id = Guid.NewGuid(),
            Type = InterviewQuestionType.Hr,
            QuestionText = "Why do you want to work here?",
            WhatGoodAnswersCover = "Specific, researched reasons.",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.InterviewQuestions.Add(question);

        var session = new InterviewSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = InterviewQuestionType.Hr,
            Status = InterviewSessionStatus.InProgress,
            SelectedQuestionIdsJson = JsonSerializer.Serialize(new[] { question.Id }),
            StartedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.InterviewSessions.Add(session);

        _db.InterviewAnswers.Add(new InterviewAnswer
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            QuestionId = question.Id,
            AnswerText = "Because of your mission and engineering culture.",
            AiScore = 7,
            AiFeedback = "Good, specific answer.",
            AnsweredAtUtc = _dateTimeProvider.UtcNow,
        });

        _db.SaveChanges();
        return (session, question.Id);
    }

    [Fact]
    public async Task Handle_OwnerRequestsTheirOwnSession_ReturnsQuestionsWithAnswerAlreadyGiven()
    {
        var userId = Guid.NewGuid();
        var (session, questionId) = SeedSessionWithOneAnsweredQuestion(userId);

        var result = await CreateHandler().Handle(new GetInterviewSessionQuery(userId, session.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Questions.Should().HaveCount(1);
        var question = result.Value.Questions.Single(q => q.QuestionId == questionId);
        question.AnswerText.Should().Be("Because of your mission and engineering culture.");
        question.AiScore.Should().Be(7);
        question.AiFeedback.Should().Be("Good, specific answer.");
    }

    [Fact]
    public async Task Handle_SessionBelongsToAnotherUser_FailsWithNotFound()
    {
        var ownerId = Guid.NewGuid();
        var (session, _) = SeedSessionWithOneAnsweredQuestion(ownerId);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new GetInterviewSessionQuery(attackerId, session.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_SessionDoesNotExist_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(new GetInterviewSessionQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
