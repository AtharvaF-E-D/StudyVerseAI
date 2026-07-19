using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.InterviewPrep.GetInterviewStats;

public sealed class GetInterviewStatsQueryHandler : IRequestHandler<GetInterviewStatsQuery, Result<InterviewStatsDto>>
{
    private readonly IAppDbContext _db;

    public GetInterviewStatsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<InterviewStatsDto>> Handle(GetInterviewStatsQuery request, CancellationToken cancellationToken)
    {
        var completedSessions = await _db.InterviewSessions
            .Where(s => s.UserId == request.UserId && s.Status == InterviewSessionStatus.Completed)
            .Select(s => new { s.Type, s.OverallScore })
            .ToListAsync(cancellationToken);

        var resumeAnalysesCount = await _db.ResumeAnalyses.CountAsync(r => r.UserId == request.UserId, cancellationToken);

        double? AverageFor(InterviewQuestionType type)
        {
            var scores = completedSessions
                .Where(s => s.Type == type && s.OverallScore.HasValue)
                .Select(s => (double)s.OverallScore!.Value)
                .ToList();

            return scores.Count == 0 ? null : Math.Round(scores.Average(), 1);
        }

        var dto = new InterviewStatsDto(
            completedSessions.Count,
            new AverageScoreByTypeDto(
                AverageFor(InterviewQuestionType.Hr),
                AverageFor(InterviewQuestionType.Technical),
                AverageFor(InterviewQuestionType.Behavioral)),
            resumeAnalysesCount);

        return Result.Success(dto);
    }
}
