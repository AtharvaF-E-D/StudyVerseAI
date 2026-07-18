using FluentAssertions;
using StudyVerse.Application.Features.MockTests.GetMockTestAttempts;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.MockTests;

namespace StudyVerse.Application.Tests.Features.MockTests.GetMockTestAttempts;

public sealed class GetMockTestAttemptsQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetMockTestAttemptsQueryHandler CreateHandler() => new(_db);

    private Guid SeedUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            DisplayName = "Student",
            AuthProvider = AuthProvider.Local,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Users.Add(user);
        _db.SaveChanges();
        return user.Id;
    }

    private MockTestAttempt SeedAttempt(Guid userId, DateTime startedAtUtc, int? score = null)
    {
        var attempt = new MockTestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateId = MockTestCatalog.All[0].Id,
            Status = score is null ? MockTestAttemptStatus.InProgress : MockTestAttemptStatus.Submitted,
            StartedAtUtc = startedAtUtc,
            SubmittedAtUtc = score is null ? null : startedAtUtc.AddMinutes(10),
            Score = score,
            CorrectCount = 0,
            TotalQuestions = 5,
            PercentileRank = score is null ? null : 60,
        };
        _db.MockTestAttempts.Add(attempt);
        _db.SaveChanges();
        return attempt;
    }

    [Fact]
    public async Task Handle_ReturnsOnlyThisUsersAttemptsNewestFirst()
    {
        var userId = SeedUser();
        var otherUserId = SeedUser();

        var older = SeedAttempt(userId, _dateTimeProvider.UtcNow.AddDays(-2), score: 40);
        var newer = SeedAttempt(userId, _dateTimeProvider.UtcNow.AddDays(-1), score: 80);
        SeedAttempt(otherUserId, _dateTimeProvider.UtcNow, score: 100);

        var result = await CreateHandler().Handle(new GetMockTestAttemptsQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].AttemptId.Should().Be(newer.Id);
        result.Value[1].AttemptId.Should().Be(older.Id);
        result.Value.Should().OnlyContain(a => a.TemplateTitle == MockTestCatalog.All[0].Title);
    }

    [Fact]
    public async Task Handle_ForAUserWithNoAttempts_ReturnsAnEmptyList()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetMockTestAttemptsQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
