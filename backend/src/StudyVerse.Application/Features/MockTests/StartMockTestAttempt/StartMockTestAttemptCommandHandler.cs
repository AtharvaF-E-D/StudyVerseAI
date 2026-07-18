using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Application.Features.Quiz.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.MockTests;

namespace StudyVerse.Application.Features.MockTests.StartMockTestAttempt;

/// <summary>
/// Picks <see cref="MockTestTemplate.QuestionCount"/> random questions from the shared
/// <see cref="QuizQuestion"/> bank for the template's category (or, for the
/// <see cref="MockTestCatalog.MixedCategory"/> pseudo-category, from all 5 real categories) and
/// creates the attempt + one placeholder <see cref="MockTestAttemptAnswer"/> row per question.
/// Unlike <c>QuizQuestionSelectionService</c>, there is no anti-repetition rule here: mock tests are
/// meant to be retaken as a full test (to track improvement/percentile over time), not treated as a
/// rotating trivia pool.
/// </summary>
public sealed class StartMockTestAttemptCommandHandler
    : IRequestHandler<StartMockTestAttemptCommand, Result<StartMockTestAttemptResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StartMockTestAttemptCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<StartMockTestAttemptResultDto>> Handle(
        StartMockTestAttemptCommand request,
        CancellationToken cancellationToken)
    {
        var template = MockTestCatalog.All.FirstOrDefault(t => t.Id == request.TemplateId);
        if (template is null)
        {
            return Result.Failure<StartMockTestAttemptResultDto>(
                "Mock test template not found.", ResultErrorType.NotFound);
        }

        var pool = template.Category == MockTestCatalog.MixedCategory
            ? await _db.QuizQuestions.ToListAsync(cancellationToken)
            : await _db.QuizQuestions.Where(q => q.Category == template.Category).ToListAsync(cancellationToken);

        var selected = pool
            .OrderBy(_ => Guid.NewGuid())
            .Take(template.QuestionCount)
            .ToList();

        if (selected.Count == 0)
        {
            return Result.Failure<StartMockTestAttemptResultDto>(
                "No questions are available for this mock test yet.", ResultErrorType.NotFound);
        }

        var now = _dateTimeProvider.UtcNow;
        var attempt = new MockTestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TemplateId = template.Id,
            Status = MockTestAttemptStatus.InProgress,
            StartedAtUtc = now,
            TotalQuestions = selected.Count,
        };
        _db.MockTestAttempts.Add(attempt);

        for (var index = 0; index < selected.Count; index++)
        {
            _db.MockTestAttemptAnswers.Add(new MockTestAttemptAnswer
            {
                Id = Guid.NewGuid(),
                AttemptId = attempt.Id,
                QuestionId = selected[index].Id,
                OrderIndex = index,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        var result = new StartMockTestAttemptResultDto(
            attempt.Id,
            template.DurationMinutes,
            attempt.StartedAtUtc,
            selected
                .Select(q => new MockTestQuestionOptionsDto(q.Id, q.QuestionText, QuizMapping.GetOptions(q)))
                .ToList());

        return Result.Success(result);
    }
}
