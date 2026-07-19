using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.StartInterviewSession;

/// <summary>
/// Selects <see cref="QuestionsPerSession"/> random questions of the requested
/// <see cref="InterviewQuestionType"/> from the small hand-seeded bank — no anti-repetition logic
/// (unlike <c>QuizQuestionSelectionService</c>), per the phase spec: the pool is small enough that
/// plain randomization is enough. The selected ids are persisted, in order, as
/// <see cref="InterviewSession.SelectedQuestionIdsJson"/>.
/// </summary>
public sealed class StartInterviewSessionCommandHandler : IRequestHandler<StartInterviewSessionCommand, Result<InterviewSessionDto>>
{
    public const int QuestionsPerSession = 5;

    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StartInterviewSessionCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<InterviewSessionDto>> Handle(StartInterviewSessionCommand request, CancellationToken cancellationToken)
    {
        var pool = await _db.InterviewQuestions
            .Where(q => q.Type == request.Type)
            .ToListAsync(cancellationToken);

        if (pool.Count == 0)
        {
            return Result.Failure<InterviewSessionDto>(
                "No interview questions are available for that category yet.",
                ResultErrorType.NotFound);
        }

        var selected = pool
            .OrderBy(_ => Guid.NewGuid())
            .Take(QuestionsPerSession)
            .ToList();

        var now = _dateTimeProvider.UtcNow;
        var session = new InterviewSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.Type,
            Status = InterviewSessionStatus.InProgress,
            SelectedQuestionIdsJson = InterviewSessionQuestionIds.Serialize(selected.Select(q => q.Id)),
            StartedAtUtc = now,
        };
        _db.InterviewSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);

        var questionDtos = selected
            .Select(q => new InterviewSessionQuestionDto(q.Id, q.QuestionText, null, null, null))
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
