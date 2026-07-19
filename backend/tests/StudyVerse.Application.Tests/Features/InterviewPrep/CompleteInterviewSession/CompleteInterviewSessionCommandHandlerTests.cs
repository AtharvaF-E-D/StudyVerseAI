using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.CompleteInterviewSession;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.InterviewPrep.CompleteInterviewSession;

public sealed class CompleteInterviewSessionCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();
    private readonly IAiChatProvider _aiChatProvider = Substitute.For<IAiChatProvider>();

    private CompleteInterviewSessionCommandHandler CreateHandler() => new(_db, _aiChatProvider, _dateTimeProvider);

    private void StubImprovementPlan(string plan) =>
        _aiChatProvider
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(new AiChatResult(plan, 20, 40));

    /// <summary>Seeds an in-progress session with <paramref name="scores"/>.Count questions, each
    /// already answered and graded with the corresponding score (0-10).</summary>
    private (Guid SessionId, List<Guid> QuestionIds) SeedSessionWithAnswers(Guid userId, InterviewQuestionType type, params int[] scores)
    {
        var questionIds = new List<Guid>();
        foreach (var _ in scores)
        {
            var question = new InterviewQuestion
            {
                Id = Guid.NewGuid(),
                Type = type,
                QuestionText = "A question",
                WhatGoodAnswersCover = "Something specific.",
                CreatedAtUtc = _dateTimeProvider.UtcNow,
            };
            _db.InterviewQuestions.Add(question);
            questionIds.Add(question.Id);
        }

        var session = new InterviewSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Status = InterviewSessionStatus.InProgress,
            SelectedQuestionIdsJson = JsonSerializer.Serialize(questionIds),
            StartedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.InterviewSessions.Add(session);

        for (var i = 0; i < scores.Length; i++)
        {
            _db.InterviewAnswers.Add(new InterviewAnswer
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                QuestionId = questionIds[i],
                AnswerText = $"Answer {i}",
                AiScore = scores[i],
                AiFeedback = $"Feedback {i}",
                AnsweredAtUtc = _dateTimeProvider.UtcNow,
            });
        }

        _db.SaveChanges();
        return (session.Id, questionIds);
    }

    [Fact]
    public async Task Handle_NotAllQuestionsAnswered_FailsValidationWithoutCallingAi()
    {
        var userId = Guid.NewGuid();
        // Seed a session with 5 selected questions but only answer 3 of them - simulate by seeding
        // the session with 5 ids but only 3 InterviewAnswer rows.
        var q1 = Guid.NewGuid();
        var q2 = Guid.NewGuid();
        var q3 = Guid.NewGuid();
        var q4 = Guid.NewGuid();
        var q5 = Guid.NewGuid();
        foreach (var id in new[] { q1, q2, q3, q4, q5 })
        {
            _db.InterviewQuestions.Add(new InterviewQuestion
            {
                Id = id,
                Type = InterviewQuestionType.Technical,
                QuestionText = "Q",
                WhatGoodAnswersCover = "Something.",
                CreatedAtUtc = _dateTimeProvider.UtcNow,
            });
        }

        var session = new InterviewSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = InterviewQuestionType.Technical,
            Status = InterviewSessionStatus.InProgress,
            SelectedQuestionIdsJson = JsonSerializer.Serialize(new[] { q1, q2, q3, q4, q5 }),
            StartedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.InterviewSessions.Add(session);

        foreach (var id in new[] { q1, q2, q3 })
        {
            _db.InterviewAnswers.Add(new InterviewAnswer
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                QuestionId = id,
                AnswerText = "answer",
                AiScore = 7,
                AiFeedback = "feedback",
                AnsweredAtUtc = _dateTimeProvider.UtcNow,
            });
        }

        _db.SaveChanges();

        var result = await CreateHandler().Handle(new CompleteInterviewSessionCommand(userId, session.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
        result.Error.Should().Contain("2");
        await _aiChatProvider.DidNotReceive().GetCompletionAsync(
            Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Handle_AllFiveAnswered_AveragesTheFiveScoresIntoAZeroToHundredOverallScore()
    {
        var userId = Guid.NewGuid();
        // Scores 10, 9, 8, 7, 6 out of 10 each -> average 8.0/10 -> overall 80/100.
        var (sessionId, _) = SeedSessionWithAnswers(userId, InterviewQuestionType.Behavioral, 10, 9, 8, 7, 6);
        StubImprovementPlan("A real, specific improvement plan referencing the candidate's actual answers in detail across two solid paragraphs.");

        var result = await CreateHandler().Handle(new CompleteInterviewSessionCommand(userId, sessionId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OverallScore.Should().Be(80);
        result.Value.Status.Should().Be(InterviewSessionStatus.Completed);
        result.Value.ImprovementPlan.Should().Contain("real, specific improvement plan");

        var persisted = await _db.InterviewSessions.SingleAsync(s => s.Id == sessionId);
        persisted.OverallScore.Should().Be(80);
        persisted.Status.Should().Be(InterviewSessionStatus.Completed);
        persisted.CompletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_LowScoringSession_AveragesCorrectlyToTheZeroToHundredScale()
    {
        var userId = Guid.NewGuid();
        // Scores 2, 2, 2, 2, 2 -> average 2.0/10 -> overall 20/100.
        var (sessionId, _) = SeedSessionWithAnswers(userId, InterviewQuestionType.Hr, 2, 2, 2, 2, 2);
        StubImprovementPlan("This candidate needs significant work across every answer given.");

        var result = await CreateHandler().Handle(new CompleteInterviewSessionCommand(userId, sessionId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OverallScore.Should().Be(20);
    }

    [Fact]
    public async Task Handle_AlreadyCompletedSession_FailsWithConflict()
    {
        var userId = Guid.NewGuid();
        var (sessionId, _) = SeedSessionWithAnswers(userId, InterviewQuestionType.Technical, 8, 8, 8, 8, 8);
        var session = await _db.InterviewSessions.SingleAsync(s => s.Id == sessionId);
        session.Status = InterviewSessionStatus.Completed;
        session.OverallScore = 80;
        session.CompletedAtUtc = _dateTimeProvider.UtcNow;
        await _db.SaveChangesAsync(CancellationToken.None);

        var result = await CreateHandler().Handle(new CompleteInterviewSessionCommand(userId, sessionId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_SessionBelongsToAnotherUser_FailsWithNotFound()
    {
        var ownerId = Guid.NewGuid();
        var (sessionId, _) = SeedSessionWithAnswers(ownerId, InterviewQuestionType.Hr, 7, 7, 7, 7, 7);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new CompleteInterviewSessionCommand(attackerId, sessionId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
