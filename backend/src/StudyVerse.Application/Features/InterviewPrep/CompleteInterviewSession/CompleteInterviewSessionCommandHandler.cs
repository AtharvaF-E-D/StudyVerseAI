using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.CompleteInterviewSession;

/// <summary>
/// Validates every selected question has a real answer (rejecting otherwise), averages the five
/// 0-10 <c>InterviewAnswer.AiScore</c> values into a 0-100
/// <c>InterviewSession.OverallScore</c> (average out of 10, scaled ×10), then makes ONE more
/// <see cref="IAiChatProvider"/> call synthesizing all five Q&amp;A pairs (plus their scores/feedback)
/// into a real, specific improvement plan. Everything — including the AI call — happens before the
/// single <c>SaveChangesAsync</c>, the same "compute in memory, commit once" shape
/// <c>SubmitMockTestAttemptCommandHandler</c> uses, so a failed AI call never leaves the session
/// half-completed.
/// </summary>
public sealed class CompleteInterviewSessionCommandHandler : IRequestHandler<CompleteInterviewSessionCommand, Result<InterviewSessionDto>>
{
    private readonly IAppDbContext _db;
    private readonly IAiChatProvider _aiChatProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CompleteInterviewSessionCommandHandler(IAppDbContext db, IAiChatProvider aiChatProvider, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _aiChatProvider = aiChatProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<InterviewSessionDto>> Handle(CompleteInterviewSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.InterviewSessions.FirstOrDefaultAsync(
            s => s.Id == request.SessionId && s.UserId == request.UserId,
            cancellationToken);

        if (session is null)
        {
            return Result.Failure<InterviewSessionDto>("Interview session not found.", ResultErrorType.NotFound);
        }

        if (session.Status != InterviewSessionStatus.InProgress)
        {
            return Result.Failure<InterviewSessionDto>(
                "This interview session has already been completed.",
                ResultErrorType.Conflict);
        }

        var questionIds = InterviewSessionQuestionIds.Deserialize(session.SelectedQuestionIdsJson);

        var questionsById = await _db.InterviewQuestions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, cancellationToken);

        var answersByQuestionId = await _db.InterviewAnswers
            .Where(a => a.SessionId == session.Id)
            .ToDictionaryAsync(a => a.QuestionId, cancellationToken);

        var unansweredCount = questionIds.Count(id => !answersByQuestionId.ContainsKey(id));
        if (unansweredCount > 0)
        {
            return Result.Failure<InterviewSessionDto>(
                $"{unansweredCount} question(s) still need an answer before this session can be completed.",
                ResultErrorType.Validation);
        }

        var orderedPairs = questionIds
            .Select(id =>
            {
                var answer = answersByQuestionId[id];
                return new GradedQaPair(questionsById[id].QuestionText, answer.AnswerText, answer.AiScore ?? 0, answer.AiFeedback ?? string.Empty);
            })
            .ToList();

        var averageOutOfTen = orderedPairs.Count == 0 ? 0 : orderedPairs.Average(p => p.Score);
        var overallScore = (int)Math.Round(averageOutOfTen * 10);

        var prompt = InterviewImprovementPlanPromptBuilder.Build(session.Type, orderedPairs);
        var completion = await _aiChatProvider.GetCompletionAsync(
            [new AiChatMessage(MessageRole.User, prompt)],
            cancellationToken);

        var now = _dateTimeProvider.UtcNow;
        session.Status = InterviewSessionStatus.Completed;
        session.OverallScore = overallScore;
        session.ImprovementPlan = completion.Content.Trim();
        session.CompletedAtUtc = now;

        await _db.SaveChangesAsync(cancellationToken);

        var questionDtos = questionIds
            .Select(id =>
            {
                var answer = answersByQuestionId[id];
                return new InterviewSessionQuestionDto(id, questionsById[id].QuestionText, answer.AnswerText, answer.AiScore, answer.AiFeedback);
            })
            .ToList();

        var dto = new InterviewSessionDto(
            session.Id,
            session.Type,
            session.Status,
            session.OverallScore,
            session.ImprovementPlan,
            session.StartedAtUtc,
            session.CompletedAtUtc,
            questionDtos);

        return Result.Success(dto);
    }
}
