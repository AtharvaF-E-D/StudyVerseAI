using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.SubmitAnswer;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.InterviewPrep.SubmitAnswer;

public sealed class SubmitAnswerCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();
    private readonly IAiChatProvider _aiChatProvider = Substitute.For<IAiChatProvider>();

    private SubmitAnswerCommandHandler CreateHandler() => new(_db, _aiChatProvider, _dateTimeProvider);

    private void StubGrading(int score, string feedback) =>
        _aiChatProvider
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(new AiChatResult(JsonSerializer.Serialize(new { score, feedback }), 10, 10));

    /// <summary>Seeds an in-progress session with the given questions already selected (mirroring
    /// what <c>StartInterviewSessionCommandHandler</c> would have persisted).</summary>
    private (Guid SessionId, List<Guid> QuestionIds) SeedInProgressSession(Guid userId, InterviewQuestionType type, int questionCount = 5)
    {
        var questionIds = new List<Guid>();
        for (var i = 0; i < questionCount; i++)
        {
            var question = new InterviewQuestion
            {
                Id = Guid.NewGuid(),
                Type = type,
                QuestionText = $"Question {i}",
                WhatGoodAnswersCover = "Specific, concrete detail relevant to the question.",
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
        _db.SaveChanges();

        return (session.Id, questionIds);
    }

    [Fact]
    public async Task Handle_ValidAnswer_GradesViaAiAndPersistsImmediately()
    {
        var userId = Guid.NewGuid();
        var (sessionId, questionIds) = SeedInProgressSession(userId, InterviewQuestionType.Behavioral);
        StubGrading(8, "Strong, specific example with a clear outcome.");

        var result = await CreateHandler().Handle(
            new SubmitAnswerCommand(userId, sessionId, questionIds[0], "A detailed, specific answer about a real project."),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Score.Should().Be(8);
        result.Value.Feedback.Should().Be("Strong, specific example with a clear outcome.");

        var persisted = await _db.InterviewAnswers.SingleAsync(a => a.SessionId == sessionId && a.QuestionId == questionIds[0]);
        persisted.AiScore.Should().Be(8);
        persisted.AiFeedback.Should().Be("Strong, specific example with a clear outcome.");
        persisted.AnswerText.Should().Be("A detailed, specific answer about a real project.");
    }

    [Fact]
    public async Task Handle_QuestionNotPartOfSession_FailsValidationWithoutCallingAi()
    {
        var userId = Guid.NewGuid();
        var (sessionId, _) = SeedInProgressSession(userId, InterviewQuestionType.Hr);
        var foreignQuestionId = Guid.NewGuid();

        var result = await CreateHandler().Handle(
            new SubmitAnswerCommand(userId, sessionId, foreignQuestionId, "Some answer"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Validation);
        await _aiChatProvider.DidNotReceive().GetCompletionAsync(
            Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Handle_SessionAlreadyCompleted_FailsWithConflict()
    {
        var userId = Guid.NewGuid();
        var (sessionId, questionIds) = SeedInProgressSession(userId, InterviewQuestionType.Technical);
        var session = await _db.InterviewSessions.SingleAsync(s => s.Id == sessionId);
        session.Status = InterviewSessionStatus.Completed;
        session.CompletedAtUtc = _dateTimeProvider.UtcNow;
        await _db.SaveChangesAsync(CancellationToken.None);

        var result = await CreateHandler().Handle(
            new SubmitAnswerCommand(userId, sessionId, questionIds[0], "Some answer"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_SessionBelongsToAnotherUser_FailsWithNotFound()
    {
        var ownerId = Guid.NewGuid();
        var (sessionId, questionIds) = SeedInProgressSession(ownerId, InterviewQuestionType.Hr);
        var attackerId = Guid.NewGuid();

        var result = await CreateHandler().Handle(
            new SubmitAnswerCommand(attackerId, sessionId, questionIds[0], "Some answer"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ResubmittingAnAlreadyAnsweredQuestion_OverwritesRatherThanDuplicates()
    {
        var userId = Guid.NewGuid();
        var (sessionId, questionIds) = SeedInProgressSession(userId, InterviewQuestionType.Hr);
        var handler = CreateHandler();

        StubGrading(3, "Too short, lacks detail.");
        await handler.Handle(new SubmitAnswerCommand(userId, sessionId, questionIds[0], "short"), CancellationToken.None);

        StubGrading(9, "Excellent, thorough, and specific.");
        var second = await handler.Handle(
            new SubmitAnswerCommand(userId, sessionId, questionIds[0], "a much longer and more thoughtful revised answer"),
            CancellationToken.None);

        second.Value.Score.Should().Be(9);
        (await _db.InterviewAnswers.CountAsync(a => a.SessionId == sessionId && a.QuestionId == questionIds[0])).Should().Be(1);
    }
}
