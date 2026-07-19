using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewSession;

/// <summary>Ownership-checked: only returns a session belonging to <c>UserId</c>. Joins the
/// session's fixed <c>SelectedQuestionIdsJson</c> ordering back against
/// <c>InterviewQuestions</c>/<c>InterviewAnswers</c> to show each question plus whatever answer/grade
/// has been given so far (null for any not yet answered).</summary>
public sealed class GetInterviewSessionQueryHandler : IRequestHandler<GetInterviewSessionQuery, Result<InterviewSessionDto>>
{
    private readonly IAppDbContext _db;

    public GetInterviewSessionQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<InterviewSessionDto>> Handle(GetInterviewSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _db.InterviewSessions.FirstOrDefaultAsync(
            s => s.Id == request.SessionId && s.UserId == request.UserId,
            cancellationToken);

        if (session is null)
        {
            return Result.Failure<InterviewSessionDto>("Interview session not found.", ResultErrorType.NotFound);
        }

        var questionIds = InterviewSessionQuestionIds.Deserialize(session.SelectedQuestionIdsJson);

        var questionsById = await _db.InterviewQuestions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, cancellationToken);

        var answersByQuestionId = await _db.InterviewAnswers
            .Where(a => a.SessionId == session.Id)
            .ToDictionaryAsync(a => a.QuestionId, cancellationToken);

        var questionDtos = questionIds
            .Where(questionsById.ContainsKey)
            .Select(id =>
            {
                var question = questionsById[id];
                answersByQuestionId.TryGetValue(id, out var answer);
                return new InterviewSessionQuestionDto(id, question.QuestionText, answer?.AnswerText, answer?.AiScore, answer?.AiFeedback);
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
