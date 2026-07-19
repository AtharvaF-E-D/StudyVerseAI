using FluentAssertions;
using StudyVerse.Application.Features.CodingPractice.GetProblems;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.CodingPractice.GetProblems;

public sealed class GetProblemsQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetProblemsQueryHandler CreateHandler() => new(_db);

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

    private Guid SeedProblem(string title, CodingDifficulty difficulty, string category, bool isInterview = false)
    {
        var problem = new CodingProblem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Some description.",
            Difficulty = difficulty,
            Category = category,
            IsInterviewQuestion = isInterview,
            StarterCodeJson = "{}",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.CodingProblems.Add(problem);
        _db.SaveChanges();
        return problem.Id;
    }

    private void SeedAcceptedSubmission(Guid userId, Guid problemId)
    {
        _db.CodeSubmissions.Add(new CodeSubmission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProblemId = problemId,
            LanguageId = 109,
            SourceCode = "solution",
            Status = CodeSubmissionStatus.Accepted,
            TestsPassed = 1,
            TotalTests = 1,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_AProblemWithAnAcceptedSubmissionForThisUser_IsFlaggedSolved()
    {
        var userId = SeedUser();
        var otherUserId = SeedUser();
        var solvedProblemId = SeedProblem("Two Sum", CodingDifficulty.Medium, "Arrays");
        var unsolvedProblemId = SeedProblem("FizzBuzz", CodingDifficulty.Easy, "Math");
        SeedAcceptedSubmission(userId, solvedProblemId);
        // Another user's accepted submission must NOT make it "solved" for this user.
        SeedAcceptedSubmission(otherUserId, unsolvedProblemId);

        var result = await CreateHandler().Handle(new GetProblemsQuery(userId, null, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Single(p => p.Id == solvedProblemId).IsSolved.Should().BeTrue();
        result.Value.Single(p => p.Id == unsolvedProblemId).IsSolved.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ANonAcceptedSubmission_DoesNotCountAsSolved()
    {
        var userId = SeedUser();
        var problemId = SeedProblem("Valid Parentheses", CodingDifficulty.Medium, "Data Structures");
        _db.CodeSubmissions.Add(new CodeSubmission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProblemId = problemId,
            LanguageId = 109,
            SourceCode = "wrong",
            Status = CodeSubmissionStatus.WrongAnswer,
            TestsPassed = 0,
            TotalTests = 3,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetProblemsQuery(userId, null, null, null), CancellationToken.None);

        result.Value.Single(p => p.Id == problemId).IsSolved.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_FilteredByDifficulty_ReturnsOnlyThatDifficulty()
    {
        var userId = SeedUser();
        SeedProblem("Easy One", CodingDifficulty.Easy, "Math");
        SeedProblem("Medium One", CodingDifficulty.Medium, "Arrays");
        SeedProblem("Hard One", CodingDifficulty.Hard, "Strings");

        var result = await CreateHandler().Handle(new GetProblemsQuery(userId, "Medium", null, null), CancellationToken.None);

        result.Value.Should().ContainSingle(p => p.Title == "Medium One");
    }

    [Fact]
    public async Task Handle_FilteredByDifficulty_IsCaseInsensitive()
    {
        var userId = SeedUser();
        SeedProblem("Easy One", CodingDifficulty.Easy, "Math");
        SeedProblem("Hard One", CodingDifficulty.Hard, "Strings");

        var result = await CreateHandler().Handle(new GetProblemsQuery(userId, "easy", null, null), CancellationToken.None);

        result.Value.Should().ContainSingle(p => p.Title == "Easy One");
    }

    [Fact]
    public async Task Handle_FilteredByCategory_ReturnsOnlyThatCategory()
    {
        var userId = SeedUser();
        SeedProblem("Reverse a String", CodingDifficulty.Easy, "Strings");
        SeedProblem("Sum of an Array", CodingDifficulty.Easy, "Arrays");

        var result = await CreateHandler().Handle(new GetProblemsQuery(userId, null, "Strings", null), CancellationToken.None);

        result.Value.Should().ContainSingle(p => p.Title == "Reverse a String");
    }

    [Fact]
    public async Task Handle_InterviewOnlyTrue_ReturnsOnlyInterviewQuestions()
    {
        var userId = SeedUser();
        SeedProblem("Two Sum", CodingDifficulty.Medium, "Arrays", isInterview: true);
        SeedProblem("FizzBuzz", CodingDifficulty.Easy, "Math", isInterview: false);

        var result = await CreateHandler().Handle(new GetProblemsQuery(userId, null, null, true), CancellationToken.None);

        result.Value.Should().ContainSingle(p => p.Title == "Two Sum");
    }

    [Fact]
    public async Task Handle_CombinedDifficultyAndCategoryFilters_NarrowsToTheIntersection()
    {
        var userId = SeedUser();
        SeedProblem("Match", CodingDifficulty.Hard, "Arrays");
        SeedProblem("Wrong Difficulty", CodingDifficulty.Easy, "Arrays");
        SeedProblem("Wrong Category", CodingDifficulty.Hard, "Strings");

        var result = await CreateHandler().Handle(new GetProblemsQuery(userId, "Hard", "Arrays", null), CancellationToken.None);

        result.Value.Should().ContainSingle(p => p.Title == "Match");
    }
}
