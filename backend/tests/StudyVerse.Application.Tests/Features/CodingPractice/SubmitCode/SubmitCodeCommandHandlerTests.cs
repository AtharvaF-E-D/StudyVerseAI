using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CodingPractice.SubmitCode;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.CodingPractice.SubmitCode;

public sealed class SubmitCodeCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();
    private readonly IJudge0Provider _judge0Provider = Substitute.For<IJudge0Provider>();

    private SubmitCodeCommandHandler CreateHandler() => new(_db, _judge0Provider, _dateTimeProvider);

    private Guid SeedUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            DisplayName = "Coder",
            AuthProvider = AuthProvider.Local,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Users.Add(user);
        _db.SaveChanges();
        return user.Id;
    }

    /// <summary>Seeds a problem with the given (input, expectedOutput, isSample) test cases, in order.</summary>
    private Guid SeedProblem(CodingDifficulty difficulty, params (string Input, string ExpectedOutput, bool IsSample)[] testCases)
    {
        var problem = new CodingProblem
        {
            Id = Guid.NewGuid(),
            Title = "Sum of an Array",
            Description = "Read space-separated integers and print their sum.",
            Difficulty = difficulty,
            Category = "Arrays",
            IsInterviewQuestion = false,
            StarterCodeJson = "{}",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.CodingProblems.Add(problem);

        for (var i = 0; i < testCases.Length; i++)
        {
            _db.CodingProblemTestCases.Add(new CodingProblemTestCase
            {
                Id = Guid.NewGuid(),
                ProblemId = problem.Id,
                Input = testCases[i].Input,
                ExpectedOutput = testCases[i].ExpectedOutput,
                IsSample = testCases[i].IsSample,
                OrderIndex = i,
            });
        }

        _db.SaveChanges();
        return problem.Id;
    }

    private void StubJudge0(string stdin, Judge0ResultDto result) =>
        _judge0Provider
            .RunAsync(Arg.Any<int>(), Arg.Any<string>(), stdin, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(result);

    [Fact]
    public async Task Handle_FirstEverAcceptedSubmission_AwardsXpAndCoinsByDifficultyAndPersistsTheSubmission()
    {
        var userId = SeedUser();
        var problemId = SeedProblem(CodingDifficulty.Easy, ("1 2 3", "6", true), ("4 5", "9", false));
        StubJudge0("1 2 3", new Judge0ResultDto("Accepted", "6", null, null));
        StubJudge0("4 5", new Judge0ResultDto("Accepted", "9", null, null));

        var result = await CreateHandler().Handle(
            new SubmitCodeCommand(userId, problemId, 109, "print(sum(map(int, input().split())))"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(CodeSubmissionStatus.Accepted);
        result.Value.TestsPassed.Should().Be(2);
        result.Value.TotalTests.Should().Be(2);
        result.Value.XpAwarded.Should().Be(15); // Easy
        result.Value.CoinsAwarded.Should().Be(3); // Easy
        result.Value.AlreadySolved.Should().BeFalse();

        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.Xp.Should().Be(15);
        progress.Coins.Should().Be(3);

        (await _db.CodeSubmissions.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_ASecondAcceptedSubmissionForAnAlreadySolvedProblem_DoesNotReAwardXpOrCoins()
    {
        var userId = SeedUser();
        var problemId = SeedProblem(CodingDifficulty.Medium, ("1 2 3", "6", true));
        StubJudge0("1 2 3", new Judge0ResultDto("Accepted", "6", null, null));
        var handler = CreateHandler();

        var first = await handler.Handle(new SubmitCodeCommand(userId, problemId, 109, "solution v1"), CancellationToken.None);
        first.Value.XpAwarded.Should().Be(25); // Medium
        first.Value.AlreadySolved.Should().BeFalse();

        var second = await handler.Handle(new SubmitCodeCommand(userId, problemId, 109, "solution v2"), CancellationToken.None);

        second.Value.Status.Should().Be(CodeSubmissionStatus.Accepted);
        second.Value.XpAwarded.Should().Be(0);
        second.Value.CoinsAwarded.Should().Be(0);
        second.Value.AlreadySolved.Should().BeTrue();

        // Total progress reflects only the FIRST accepted solve, never counted twice.
        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.Xp.Should().Be(25);
        progress.Coins.Should().Be(5);

        (await _db.CodeSubmissions.CountAsync(s => s.UserId == userId && s.ProblemId == problemId)).Should().Be(2);
    }

    [Fact]
    public async Task Handle_AWrongAnswerOnAHiddenTestCase_ReturnsWrongAnswerWithoutLeakingTheHiddenTestCasesContent()
    {
        var userId = SeedUser();
        var problemId = SeedProblem(
            CodingDifficulty.Easy,
            ("sample-in", "sample-out", true),
            ("hidden-in", "hidden-out", false));

        StubJudge0("sample-in", new Judge0ResultDto("Accepted", "sample-out", null, null));
        StubJudge0("hidden-in", new Judge0ResultDto("Wrong Answer", "actually this is wrong", null, null));

        var result = await CreateHandler().Handle(
            new SubmitCodeCommand(userId, problemId, 109, "a broken solution"),
            CancellationToken.None);

        result.Value.Status.Should().Be(CodeSubmissionStatus.WrongAnswer);
        result.Value.TestsPassed.Should().Be(1);
        result.Value.TotalTests.Should().Be(2);
        result.Value.XpAwarded.Should().Be(0);
        result.Value.CoinsAwarded.Should().Be(0);

        var sampleResult = result.Value.Results.Single(r => r.IsSample);
        sampleResult.Passed.Should().BeTrue();
        sampleResult.Input.Should().Be("sample-in");
        sampleResult.ExpectedOutput.Should().Be("sample-out");
        sampleResult.ActualOutput.Should().Be("sample-out");

        // The critical anti-cheating assertion: the hidden test case's real input/expected/actual
        // content must never appear in the result the client sees - only pass/fail.
        var hiddenResult = result.Value.Results.Single(r => !r.IsSample);
        hiddenResult.Passed.Should().BeFalse();
        hiddenResult.Input.Should().BeNull();
        hiddenResult.ExpectedOutput.Should().BeNull();
        hiddenResult.ActualOutput.Should().BeNull();

        (await _db.UserProgresses.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Handle_ACompileError_ShortCircuitsAndNeverRunsTheRemainingTestCases()
    {
        var userId = SeedUser();
        var problemId = SeedProblem(
            CodingDifficulty.Hard,
            ("first-in", "first-out", true),
            ("second-in", "second-out", false));

        StubJudge0("first-in", new Judge0ResultDto("Compilation Error", null, null, "SyntaxError: invalid syntax"));

        var result = await CreateHandler().Handle(
            new SubmitCodeCommand(userId, problemId, 109, "this doesn't even parse ("),
            CancellationToken.None);

        result.Value.Status.Should().Be(CodeSubmissionStatus.CompileError);
        result.Value.TestsPassed.Should().Be(0);
        result.Value.TotalTests.Should().Be(2);
        // Only the first test case was ever attempted - the run stopped before the second.
        result.Value.Results.Should().HaveCount(1);
        await _judge0Provider.DidNotReceive().RunAsync(Arg.Any<int>(), Arg.Any<string>(), "second-in", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Judge0ItselfUnreachable_ReturnsErrorStatusInsteadOfThrowing()
    {
        var userId = SeedUser();
        var problemId = SeedProblem(CodingDifficulty.Easy, ("1 2", "3", true));
        StubJudge0("1 2", new Judge0ResultDto("Error", null, null, null));

        var result = await CreateHandler().Handle(
            new SubmitCodeCommand(userId, problemId, 109, "print(1)"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(CodeSubmissionStatus.Error);
    }

    [Fact]
    public async Task Handle_ProblemDoesNotExist_FailsWithNotFound()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(
            new SubmitCodeCommand(userId, Guid.NewGuid(), 109, "print(1)"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
