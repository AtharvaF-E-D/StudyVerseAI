using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Application.Features.Quiz.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.MockTests;

namespace StudyVerse.Application.Features.MockTests.GetMockTestReview;

public sealed class GetMockTestReviewQueryHandler : IRequestHandler<GetMockTestReviewQuery, Result<MockTestReviewDto>>
{
    private readonly IAppDbContext _db;

    public GetMockTestReviewQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MockTestReviewDto>> Handle(GetMockTestReviewQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _db.MockTestAttempts.FirstOrDefaultAsync(
            a => a.Id == request.AttemptId && a.UserId == request.UserId,
            cancellationToken);

        if (attempt is null)
        {
            return Result.Failure<MockTestReviewDto>("Mock test attempt not found.", ResultErrorType.NotFound);
        }

        if (attempt.Status != MockTestAttemptStatus.Submitted)
        {
            return Result.Failure<MockTestReviewDto>(
                "A review is only available for submitted mock test attempts.",
                ResultErrorType.Validation);
        }

        var rows = await (
            from a in _db.MockTestAttemptAnswers
            join q in _db.QuizQuestions on a.QuestionId equals q.Id
            where a.AttemptId == attempt.Id
            orderby a.OrderIndex
            select new { a, q }
        ).ToListAsync(cancellationToken);

        var template = MockTestCatalog.All.FirstOrDefault(t => t.Id == attempt.TemplateId);

        var questions = rows
            .Select(row => new MockTestReviewQuestionDto(
                row.q.Id,
                row.a.OrderIndex,
                row.q.QuestionText,
                QuizMapping.GetOptions(row.q),
                row.q.CorrectOptionIndex,
                row.a.SelectedOptionIndex,
                row.a.IsCorrect,
                row.q.Explanation))
            .ToList();

        var result = new MockTestReviewDto(
            attempt.Id,
            attempt.TemplateId,
            template?.Title ?? "Unknown Mock Test",
            attempt.Score ?? 0,
            attempt.CorrectCount,
            attempt.TotalQuestions,
            attempt.PercentileRank,
            attempt.AiWeaknessAnalysis,
            questions);

        return Result.Success(result);
    }
}
