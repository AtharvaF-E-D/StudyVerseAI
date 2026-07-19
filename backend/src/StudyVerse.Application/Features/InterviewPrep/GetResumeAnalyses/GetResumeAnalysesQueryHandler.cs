using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.InterviewPrep.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.InterviewPrep.GetResumeAnalyses;

public sealed class GetResumeAnalysesQueryHandler
    : IRequestHandler<GetResumeAnalysesQuery, Result<IReadOnlyList<ResumeAnalysisDto>>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAppDbContext _db;

    public GetResumeAnalysesQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<ResumeAnalysisDto>>> Handle(
        GetResumeAnalysesQuery request,
        CancellationToken cancellationToken)
    {
        var analyses = await _db.ResumeAnalyses
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.AnalyzedAtUtc)
            .ToListAsync(cancellationToken);

        var dtos = analyses
            .Select(r => new ResumeAnalysisDto(
                r.Id,
                r.FileName,
                r.OverallScore,
                Deserialize(r.StrengthsJson),
                Deserialize(r.WeaknessesJson),
                Deserialize(r.SuggestionsJson),
                r.AnalyzedAtUtc))
            .ToList();

        return Result.Success<IReadOnlyList<ResumeAnalysisDto>>(dtos);
    }

    private static IReadOnlyList<string> Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
