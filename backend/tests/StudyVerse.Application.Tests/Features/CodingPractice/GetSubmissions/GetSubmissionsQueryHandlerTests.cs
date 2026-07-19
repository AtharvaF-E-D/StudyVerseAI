using FluentAssertions;
using StudyVerse.Application.Features.CodingPractice.GetSubmissions;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.CodingPractice.GetSubmissions;

public sealed class GetSubmissionsQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetSubmissionsQueryHandler CreateHandler() => new(_db);

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

    private Guid SeedProblem(string title = "FizzBuzz")
    {
        var problem = new CodingProblem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "desc",
            Difficulty = CodingDifficulty.Easy,
            Category = "Math",
            StarterCodeJson = "{}",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.CodingProblems.Add(problem);
        _db.SaveChanges();
        return problem.Id;
    }

    private void SeedSubmission(Guid userId, Guid problemId, DateTime submittedAtUtc, CodeSubmissionStatus status = CodeSubmissionStatus.Accepted)
    {
        _db.CodeSubmissions.Add(new CodeSubmission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProblemId = problemId,
            LanguageId = 109,
            SourceCode = "code",
            Status = status,
            TestsPassed = 1,
            TotalTests = 1,
            SubmittedAtUtc = submittedAtUtc,
        });
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_ReturnsOnlyTheRequestingUsersSubmissions_NeverAnotherUsersSubmissionsForTheSameProblem()
    {
        var userId = SeedUser();
        var otherUserId = SeedUser();
        var problemId = SeedProblem();
        SeedSubmission(userId, problemId, _dateTimeProvider.UtcNow);
        SeedSubmission(otherUserId, problemId, _dateTimeProvider.UtcNow);

        var result = await CreateHandler().Handle(new GetSubmissionsQuery(userId, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value.Single().ProblemId.Should().Be(problemId);
    }

    [Fact]
    public async Task Handle_FilteredByProblemId_OnlyReturnsThatProblemsSubmissionsForThisUser()
    {
        var userId = SeedUser();
        var problemA = SeedProblem("Problem A");
        var problemB = SeedProblem("Problem B");
        SeedSubmission(userId, problemA, _dateTimeProvider.UtcNow);
        SeedSubmission(userId, problemB, _dateTimeProvider.UtcNow);

        var result = await CreateHandler().Handle(new GetSubmissionsQuery(userId, problemA), CancellationToken.None);

        result.Value.Should().ContainSingle(s => s.ProblemId == problemA);
    }

    [Fact]
    public async Task Handle_NoProblemIdFilter_ReturnsAllOfThisUsersSubmissionsNewestFirst()
    {
        var userId = SeedUser();
        var problemId = SeedProblem();
        var older = _dateTimeProvider.UtcNow.AddDays(-2);
        var newer = _dateTimeProvider.UtcNow;
        SeedSubmission(userId, problemId, older);
        SeedSubmission(userId, problemId, newer);

        var result = await CreateHandler().Handle(new GetSubmissionsQuery(userId, null), CancellationToken.None);

        result.Value.Should().HaveCount(2);
        result.Value[0].SubmittedAtUtc.Should().Be(newer);
        result.Value[1].SubmittedAtUtc.Should().Be(older);
    }
}
