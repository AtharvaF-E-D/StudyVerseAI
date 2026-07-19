using FluentAssertions;
using StudyVerse.Application.Features.Gamification.ClaimDailyReward;
using StudyVerse.Application.Features.Gamification.GetDailyRewardStatus;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.Gamification.GetDailyRewardStatus;

public sealed class GetDailyRewardStatusQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetDailyRewardStatusQueryHandler CreateHandler() => new(_db, _dateTimeProvider);

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
    public async Task Handle_BeforeAnyClaim_ReportsNotClaimedAndDayOnePreview()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetDailyRewardStatusQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClaimedToday.Should().BeFalse();
        result.Value.DayNumber.Should().Be(1);
        result.Value.TodayCoins.Should().Be(10);
        result.Value.TomorrowCoins.Should().Be(15);
    }

    [Fact]
    public async Task Handle_AfterClaimingToday_ReportsClaimedTodayWithTheClaimedDayNumber()
    {
        var userId = SeedUser();
        await new ClaimDailyRewardCommandHandler(_db, _dateTimeProvider)
            .Handle(new ClaimDailyRewardCommand(userId), CancellationToken.None);

        var result = await CreateHandler().Handle(new GetDailyRewardStatusQuery(userId), CancellationToken.None);

        result.Value.ClaimedToday.Should().BeTrue();
        result.Value.DayNumber.Should().Be(1);
    }
}
