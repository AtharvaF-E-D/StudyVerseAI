using FluentAssertions;
using StudyVerse.Application.Features.MockTests.GetMockTestAttempt;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.MockTests;

namespace StudyVerse.Application.Tests.Features.MockTests.GetMockTestAttempt;

public sealed class GetMockTestAttemptQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetMockTestAttemptQueryHandler CreateHandler() => new(_db);

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

    [Fact]
    public async Task Handle_ForASubmittedAttemptOwnedByTheCaller_ReturnsItsSummary()
    {
        var userId = SeedUser();
        var template = MockTestCatalog.All[0];
        var attempt = new MockTestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateId = template.Id,
            Status = MockTestAttemptStatus.Submitted,
            StartedAtUtc = _dateTimeProvider.UtcNow,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
            Score = 80,
            CorrectCount = 4,
            TotalQuestions = 5,
            PercentileRank = 75,
            AiWeaknessAnalysis = "Work on X.",
        };
        _db.MockTestAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        var result = await CreateHandler().Handle(new GetMockTestAttemptQuery(userId, attempt.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TemplateTitle.Should().Be(template.Title);
        result.Value.Category.Should().Be(template.Category);
        result.Value.Score.Should().Be(80);
        result.Value.CorrectCount.Should().Be(4);
        result.Value.PercentileRank.Should().Be(75);
        result.Value.AiWeaknessAnalysis.Should().Be("Work on X.");
    }

    [Fact]
    public async Task Handle_ForAnAttemptOwnedByAnotherUser_FailsWithNotFound()
    {
        var ownerId = SeedUser();
        var otherUserId = SeedUser();
        var attempt = new MockTestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            TemplateId = MockTestCatalog.All[0].Id,
            Status = MockTestAttemptStatus.InProgress,
            StartedAtUtc = _dateTimeProvider.UtcNow,
            TotalQuestions = 5,
        };
        _db.MockTestAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        var result = await CreateHandler().Handle(new GetMockTestAttemptQuery(otherUserId, attempt.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ForAnAttemptThatDoesNotExist_FailsWithNotFound()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetMockTestAttemptQuery(userId, Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }
}
