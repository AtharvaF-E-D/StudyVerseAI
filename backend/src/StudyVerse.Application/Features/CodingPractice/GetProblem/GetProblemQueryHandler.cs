using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.CodingPractice.GetProblem;

/// <summary>
/// Returns a problem's full detail for solving: description, ONLY its <c>IsSample</c> test cases
/// (hidden ones are never leaked - see <c>CodingProblemTestCase</c>'s doc comment), and starter
/// code for the requested language. If <see cref="GetProblemQuery.LanguageId"/> has no starter code
/// entry in the problem's <c>StarterCodeJson</c>, falls back to
/// <see cref="SupportedLanguages.DefaultLanguageId"/> (Python) - documented here and on
/// <see cref="SupportedLanguages"/> - and <see cref="CodingProblemDetailDto.StarterLanguageId"/>
/// reports whichever language the returned starter code actually belongs to, so the client can tell
/// a fallback happened.
/// </summary>
public sealed class GetProblemQueryHandler : IRequestHandler<GetProblemQuery, Result<CodingProblemDetailDto>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAppDbContext _db;

    public GetProblemQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<CodingProblemDetailDto>> Handle(GetProblemQuery request, CancellationToken cancellationToken)
    {
        var problem = await _db.CodingProblems
            .Include(p => p.TestCases)
            .FirstOrDefaultAsync(p => p.Id == request.ProblemId, cancellationToken);

        if (problem is null)
        {
            return Result.Failure<CodingProblemDetailDto>("Problem not found.", ResultErrorType.NotFound);
        }

        var sampleTestCases = problem.TestCases
            .Where(t => t.IsSample)
            .OrderBy(t => t.OrderIndex)
            .Select(t => new SampleTestCaseDto(t.Input, t.ExpectedOutput))
            .ToList();

        var starters = JsonSerializer.Deserialize<Dictionary<string, string>>(problem.StarterCodeJson, JsonOptions)
            ?? new Dictionary<string, string>();

        var requestedKey = request.LanguageId.ToString();
        var (effectiveLanguageId, starterCode) = starters.TryGetValue(requestedKey, out var requestedStarter)
            ? (request.LanguageId, requestedStarter)
            : starters.TryGetValue(SupportedLanguages.DefaultLanguageId.ToString(), out var defaultStarter)
                ? (SupportedLanguages.DefaultLanguageId, defaultStarter)
                : (SupportedLanguages.DefaultLanguageId, string.Empty);

        var isSolved = await _db.CodeSubmissions.AnyAsync(
            s => s.UserId == request.UserId && s.ProblemId == problem.Id && s.Status == CodeSubmissionStatus.Accepted,
            cancellationToken);

        var dto = new CodingProblemDetailDto(
            problem.Id,
            problem.Title,
            problem.Description,
            problem.Difficulty,
            problem.Category,
            problem.IsInterviewQuestion,
            sampleTestCases,
            effectiveLanguageId,
            starterCode,
            isSolved);

        return Result.Success(dto);
    }
}
