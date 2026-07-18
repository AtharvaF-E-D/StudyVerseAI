using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.MockTests;

namespace StudyVerse.Application.Features.MockTests.GetMockTestAttempts;

public sealed class GetMockTestAttemptsQueryHandler
    : IRequestHandler<GetMockTestAttemptsQuery, Result<IReadOnlyList<MockTestAttemptListItemDto>>>
{
    private readonly IAppDbContext _db;

    public GetMockTestAttemptsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<MockTestAttemptListItemDto>>> Handle(
        GetMockTestAttemptsQuery request,
        CancellationToken cancellationToken)
    {
        var attempts = await _db.MockTestAttempts
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.StartedAtUtc)
            .ToListAsync(cancellationToken);

        var items = attempts
            .Select(a =>
            {
                var template = MockTestCatalog.All.FirstOrDefault(t => t.Id == a.TemplateId);
                return new MockTestAttemptListItemDto(
                    a.Id,
                    a.TemplateId,
                    template?.Title ?? "Unknown Mock Test",
                    a.Status,
                    a.Score,
                    a.CorrectCount,
                    a.TotalQuestions,
                    a.PercentileRank,
                    a.StartedAtUtc,
                    a.SubmittedAtUtc);
            })
            .ToList();

        return Result.Success<IReadOnlyList<MockTestAttemptListItemDto>>(items);
    }
}
