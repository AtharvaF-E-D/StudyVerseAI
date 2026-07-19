using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.SubmitAnswer;

/// <summary>
/// Real-time AI grading: one <see cref="IAiChatProvider"/> JSON-mode call per answer, graded against
/// <see cref="InterviewQuestion.WhatGoodAnswersCover"/> (never shown to the candidate) — reusing the
/// tutor's chat provider exactly like <c>GetArticleQuizQueryHandler</c>/
/// <c>SubmitMockTestAttemptCommandHandler</c> do, no new AI abstraction for this feature. Persists
/// immediately so the candidate sees feedback question-by-question, unlike
/// <c>CompleteInterviewSessionCommandHandler</c>'s "compute everything, save once" shape. Rejects a
/// question that isn't part of this session, and rejects submitting to an already-<c>Completed</c>
/// session.
/// </summary>
public sealed class SubmitAnswerCommandHandler : IRequestHandler<SubmitAnswerCommand, Result<SubmitAnswerResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IAiChatProvider _aiChatProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SubmitAnswerCommandHandler(IAppDbContext db, IAiChatProvider aiChatProvider, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _aiChatProvider = aiChatProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SubmitAnswerResultDto>> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.InterviewSessions.FirstOrDefaultAsync(
            s => s.Id == request.SessionId && s.UserId == request.UserId,
            cancellationToken);

        if (session is null)
        {
            return Result.Failure<SubmitAnswerResultDto>("Interview session not found.", ResultErrorType.NotFound);
        }

        if (session.Status != InterviewSessionStatus.InProgress)
        {
            return Result.Failure<SubmitAnswerResultDto>(
                "This interview session has already been completed.",
                ResultErrorType.Conflict);
        }

        var questionIds = InterviewSessionQuestionIds.Deserialize(session.SelectedQuestionIdsJson);
        if (!questionIds.Contains(request.QuestionId))
        {
            return Result.Failure<SubmitAnswerResultDto>(
                "That question is not part of this interview session.",
                ResultErrorType.Validation);
        }

        var question = await _db.InterviewQuestions.FirstOrDefaultAsync(
            q => q.Id == request.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure<SubmitAnswerResultDto>("Question not found.", ResultErrorType.NotFound);
        }

        var prompt = InterviewAnswerGradingPromptBuilder.Build(question.QuestionText, question.WhatGoodAnswersCover, request.AnswerText);

        var completion = await _aiChatProvider.GetCompletionAsync(
            [new AiChatMessage(MessageRole.User, prompt)],
            cancellationToken,
            requireJsonObjectResponse: true);

        var graded = InterviewAnswerGradingResponseParser.Parse(completion.Content);

        var existingAnswer = await _db.InterviewAnswers.FirstOrDefaultAsync(
            a => a.SessionId == session.Id && a.QuestionId == request.QuestionId,
            cancellationToken);

        var now = _dateTimeProvider.UtcNow;

        if (existingAnswer is not null)
        {
            // Re-answering a question already answered in this session (e.g. the candidate revises
            // their answer before completing) re-grades and overwrites in place, never duplicates.
            existingAnswer.AnswerText = request.AnswerText;
            existingAnswer.AiScore = graded.Score;
            existingAnswer.AiFeedback = graded.Feedback;
            existingAnswer.AnsweredAtUtc = now;
        }
        else
        {
            _db.InterviewAnswers.Add(new InterviewAnswer
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                QuestionId = request.QuestionId,
                AnswerText = request.AnswerText,
                AiScore = graded.Score,
                AiFeedback = graded.Feedback,
                AnsweredAtUtc = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new SubmitAnswerResultDto(graded.Score, graded.Feedback));
    }
}
