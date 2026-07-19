using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Domain.CodingPractice;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.CodingPractice.SubmitCode;

/// <summary>
/// Runs the submitted source code against every one of the problem's test cases (sample AND
/// hidden) via <see cref="IJudge0Provider"/>, one Judge0 call per test case, in
/// <see cref="Domain.Entities.CodingProblemTestCase.OrderIndex"/> order. The first test case whose
/// Judge0 verdict is a compile or runtime failure (or a Judge0-unreachable "Error") stops the run
/// immediately - the remaining test cases are never run/reported, matching how a real judge
/// short-circuits once the submission can't even execute correctly. Otherwise every test case runs,
/// and the overall status is <see cref="CodeSubmissionStatus.Accepted"/> only if every single one
/// passed, else <see cref="CodeSubmissionStatus.WrongAnswer"/>.
///
/// XP/coins (<see cref="CodingScoring"/>, by difficulty) are credited to <see cref="UserProgress"/>
/// only on the user's FIRST-EVER Accepted submission for this problem - checked by querying prior
/// submission history before persisting this one, so resubmitting an already-solved problem never
/// double-awards (<see cref="SubmitCodeResultDto.AlreadySolved"/> tells the client that happened).
///
/// Anti-cheating: <see cref="TestCaseResultDto"/> only includes the real input/expected/actual
/// content for <c>IsSample</c> test cases - hidden test case results reveal pass/fail only, the
/// same reasoning quiz answers are hidden until after submission.
/// </summary>
public sealed class SubmitCodeCommandHandler : IRequestHandler<SubmitCodeCommand, Result<SubmitCodeResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IJudge0Provider _judge0Provider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SubmitCodeCommandHandler(IAppDbContext db, IJudge0Provider judge0Provider, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _judge0Provider = judge0Provider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SubmitCodeResultDto>> Handle(SubmitCodeCommand request, CancellationToken cancellationToken)
    {
        var problem = await _db.CodingProblems
            .Include(p => p.TestCases)
            .FirstOrDefaultAsync(p => p.Id == request.ProblemId, cancellationToken);

        if (problem is null)
        {
            return Result.Failure<SubmitCodeResultDto>("Problem not found.", ResultErrorType.NotFound);
        }

        var testCases = problem.TestCases.OrderBy(t => t.OrderIndex).ToList();

        var results = new List<TestCaseResultDto>();
        var passedCount = 0;
        CodeSubmissionStatus? shortCircuitStatus = null;

        foreach (var testCase in testCases)
        {
            var judge0Result = await _judge0Provider.RunAsync(
                request.LanguageId, request.SourceCode, testCase.Input, testCase.ExpectedOutput, cancellationToken);

            var outcome = Classify(judge0Result.Status);

            if (outcome is TestOutcome.CompileError or TestOutcome.RuntimeError or TestOutcome.ProviderError)
            {
                shortCircuitStatus = outcome switch
                {
                    TestOutcome.CompileError => CodeSubmissionStatus.CompileError,
                    TestOutcome.RuntimeError => CodeSubmissionStatus.RuntimeError,
                    _ => CodeSubmissionStatus.Error,
                };

                results.Add(ToResultDto(testCase, judge0Result, passed: false));
                break;
            }

            var passed = outcome == TestOutcome.Pass;
            if (passed)
            {
                passedCount++;
            }

            results.Add(ToResultDto(testCase, judge0Result, passed));
        }

        var finalStatus = shortCircuitStatus ?? (passedCount == testCases.Count ? CodeSubmissionStatus.Accepted : CodeSubmissionStatus.WrongAnswer);

        var xpAwarded = 0;
        var coinsAwarded = 0;
        var alreadySolved = false;

        if (finalStatus == CodeSubmissionStatus.Accepted)
        {
            var hasPriorAccepted = await _db.CodeSubmissions.AnyAsync(
                s => s.UserId == request.UserId && s.ProblemId == request.ProblemId && s.Status == CodeSubmissionStatus.Accepted,
                cancellationToken);

            if (hasPriorAccepted)
            {
                alreadySolved = true;
            }
            else
            {
                xpAwarded = CodingScoring.GetXpReward(problem.Difficulty);
                coinsAwarded = CodingScoring.GetCoinReward(problem.Difficulty);

                var progress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
                if (progress is null)
                {
                    progress = new UserProgress { UserId = request.UserId };
                    _db.UserProgresses.Add(progress);
                }

                progress.Xp += xpAwarded;
                progress.Coins += coinsAwarded;
            }
        }

        _db.CodeSubmissions.Add(new CodeSubmission
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ProblemId = request.ProblemId,
            LanguageId = request.LanguageId,
            SourceCode = request.SourceCode,
            Status = finalStatus,
            TestsPassed = passedCount,
            TotalTests = testCases.Count,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
        });

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new SubmitCodeResultDto(finalStatus, passedCount, testCases.Count, results, xpAwarded, coinsAwarded, alreadySolved));
    }

    private static TestCaseResultDto ToResultDto(Domain.Entities.CodingProblemTestCase testCase, Judge0ResultDto judge0Result, bool passed) =>
        new(
            testCase.IsSample ? testCase.Input : null,
            testCase.IsSample ? testCase.ExpectedOutput : null,
            testCase.IsSample ? (judge0Result.Stdout ?? judge0Result.CompileOutput ?? judge0Result.Stderr) : null,
            passed,
            testCase.IsSample);

    private enum TestOutcome
    {
        Pass,
        Fail,
        CompileError,
        RuntimeError,
        ProviderError,
    }

    /// <summary>Maps Judge0's free-text <c>status.description</c> to a <see cref="TestOutcome"/>.
    /// <c>"Error"</c> is StudyVerse's own sentinel (see <see cref="IJudge0Provider"/>) meaning Judge0
    /// itself couldn't be reached at all - distinct from Judge0's own "Internal Error" status, which
    /// is treated as a runtime failure of the submission itself.</summary>
    private static TestOutcome Classify(string judge0Status) => judge0Status switch
    {
        "Accepted" => TestOutcome.Pass,
        "Wrong Answer" => TestOutcome.Fail,
        "Error" => TestOutcome.ProviderError,
        _ when judge0Status.StartsWith("Compilation", StringComparison.OrdinalIgnoreCase) => TestOutcome.CompileError,
        // Covers "Runtime Error (...)", "Time Limit Exceeded", "Internal Error", "Exec Format Error",
        // and any other Judge0 status that means the submission itself failed to produce a normal
        // graded result.
        _ => TestOutcome.RuntimeError,
    };
}
