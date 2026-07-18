using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.MockTests;

namespace StudyVerse.Application.Features.MockTests.GetMockTestAttempt;

public sealed class GetMockTestAttemptQueryHandler : IRequestHandler<GetMockTestAttemptQuery, Result<MockTestAttemptDetailDto>>
{
    private readonly IAppDbContext _db;

    public GetMockTestAttemptQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MockTestAttemptDetailDto>> Handle(GetMockTestAttemptQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _db.MockTestAttempts.FirstOrDefaultAsync(
            a => a.Id == request.AttemptId && a.UserId == request.UserId,
            cancellationToken);

        if (attempt is null)
        {
            return Result.Failure<MockTestAttemptDetailDto>("Mock test attempt not found.", ResultErrorType.NotFound);
        }

        var template = MockTestCatalog.All.FirstOrDefault(t => t.Id == attempt.TemplateId);

        var result = new MockTestAttemptDetailDto(
            attempt.Id,
            attempt.TemplateId,
            template?.Title ?? "Unknown Mock Test",
            template?.Category ?? string.Empty,
            attempt.Status,
            attempt.StartedAtUtc,
            attempt.SubmittedAtUtc,
            attempt.Score,
            attempt.CorrectCount,
            attempt.TotalQuestions,
            attempt.PercentileRank,
            attempt.AiWeaknessAnalysis);

        return Result.Success(result);
    }
}
