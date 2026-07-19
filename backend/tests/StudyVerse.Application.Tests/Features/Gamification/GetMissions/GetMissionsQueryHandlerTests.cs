using FluentAssertions;
using StudyVerse.Application.Features.Gamification.Common;
using StudyVerse.Application.Features.Gamification.GetMissions;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.Gamification.GetMissions;

public sealed class GetMissionsQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetMissionsQueryHandler CreateHandler() => new(_db, _dateTimeProvider, new MissionProgressService(_db, _dateTimeProvider));

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
    public async Task Handle_UnknownUser_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(new GetMissionsQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WithNoActivity_ReturnsThreeActiveMissionsAllAtZeroProgress()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetMissionsQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(3);
        result.Value.Missions.Should().HaveCount(3);
        result.Value.CompletedCount.Should().Be(0);
        result.Value.Missions.Should().OnlyContain(m => m.CurrentCount == 0 && !m.IsCompleted);
    }
}
