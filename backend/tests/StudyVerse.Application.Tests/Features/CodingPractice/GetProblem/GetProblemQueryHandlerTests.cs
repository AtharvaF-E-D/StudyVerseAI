using FluentAssertions;
using StudyVerse.Application.Features.CodingPractice.GetProblem;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.CodingPractice.GetProblem;

public sealed class GetProblemQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetProblemQueryHandler CreateHandler() => new(_db);

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

    private Guid SeedProblemWithSampleAndHiddenCases()
    {
        var problem = new CodingProblem
        {
            Id = Guid.NewGuid(),
            Title = "Sum of an Array",
            Description = "Read space-separated integers and print their sum.",
            Difficulty = CodingDifficulty.Easy,
            Category = "Arrays",
            StarterCodeJson = """{"109": "arr = list(map(int, input().split()))"}""",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.CodingProblems.Add(problem);
        _db.CodingProblemTestCases.Add(new CodingProblemTestCase
        {
            Id = Guid.NewGuid(),
            ProblemId = problem.Id,
            Input = "1 2 3",
            ExpectedOutput = "6",
            IsSample = true,
            OrderIndex = 0,
        });
        _db.CodingProblemTestCases.Add(new CodingProblemTestCase
        {
            Id = Guid.NewGuid(),
            ProblemId = problem.Id,
            Input = "secret-hidden-input",
            ExpectedOutput = "secret-hidden-output",
            IsSample = false,
            OrderIndex = 1,
        });
        _db.SaveChanges();
        return problem.Id;
    }

    [Fact]
    public async Task Handle_NeverReturnsHiddenTestCases_OnlySampleOnes()
    {
        var userId = SeedUser();
        var problemId = SeedProblemWithSampleAndHiddenCases();

        var result = await CreateHandler().Handle(new GetProblemQuery(userId, problemId, 109), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SampleTestCases.Should().ContainSingle(t => t.Input == "1 2 3" && t.ExpectedOutput == "6");
        result.Value.SampleTestCases.Should().NotContain(t => t.Input == "secret-hidden-input");
    }

    [Fact]
    public async Task Handle_RequestedLanguageHasStarterCode_ReturnsItForThatLanguage()
    {
        var userId = SeedUser();
        var problemId = SeedProblemWithSampleAndHiddenCases();

        var result = await CreateHandler().Handle(new GetProblemQuery(userId, problemId, 109), CancellationToken.None);

        result.Value.StarterLanguageId.Should().Be(109);
        result.Value.StarterCode.Should().Contain("arr = list");
    }

    [Fact]
    public async Task Handle_RequestedLanguageHasNoStarterCode_FallsBackToTheDefaultLanguage()
    {
        var userId = SeedUser();
        var problemId = SeedProblemWithSampleAndHiddenCases();

        // 91 = Java, which this problem's StarterCodeJson has no entry for.
        var result = await CreateHandler().Handle(new GetProblemQuery(userId, problemId, 91), CancellationToken.None);

        result.Value.StarterLanguageId.Should().Be(109);
        result.Value.StarterCode.Should().Contain("arr = list");
    }

    [Fact]
    public async Task Handle_ProblemDoesNotExist_FailsWithNotFound()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetProblemQuery(userId, Guid.NewGuid(), 109), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
