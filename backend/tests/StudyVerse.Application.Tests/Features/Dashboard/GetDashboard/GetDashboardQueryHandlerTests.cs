using FluentAssertions;
using StudyVerse.Application.Features.Dashboard.GetDashboard;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Features.Dashboard.GetDashboard;

public sealed class GetDashboardQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetDashboardQueryHandler CreateHandler() => new(_db, _dateTimeProvider);

    private DateOnly Today => DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

    private Guid SeedUser(string? displayName = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            DisplayName = displayName ?? "Student",
            AuthProvider = AuthProvider.Local,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Users.Add(user);
        _db.SaveChanges();
        return user.Id;
    }

    [Fact]
    public async Task Handle_ForABrandNewUserWithNoCompletionsYet_ReturnsAnEmptyZeroState()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetDashboardQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dashboard = result.Value;

        dashboard.Xp.Should().Be(0);
        dashboard.Coins.Should().Be(0);
        dashboard.Level.Should().Be(1);

        dashboard.Streak.CurrentDays.Should().Be(0);
        dashboard.Streak.LongestDays.Should().Be(0);
        dashboard.Streak.StudiedToday.Should().BeFalse();

        dashboard.TodaysChallenges.Should().HaveCount(3);
        dashboard.TodaysChallenges.Should().OnlyContain(c => !c.IsCompleted);

        dashboard.WeeklyActivity.Should().HaveCount(7);
        dashboard.WeeklyActivity.Should().OnlyContain(d => d.XpEarned == 0);
        dashboard.WeeklyActivity[^1].Date.Should().Be(Today);
        dashboard.WeeklyActivity[0].Date.Should().Be(Today.AddDays(-6));

        dashboard.Leaderboard.MyRank.Should().Be(1);
        dashboard.Leaderboard.Top.Should().ContainSingle(e => e.UserId == userId);

        dashboard.Notifications.UnreadCount.Should().Be(0);
        dashboard.Notifications.Recent.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ForAnUnknownUser_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new GetDashboardQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WithACompletedChallengeToday_MarksItCompletedAndReflectsItInWeeklyActivityAndStreak()
    {
        var userId = SeedUser();
        var template = DailyChallengeSelector.GetTodaysTemplates(Today)[0];

        _db.ChallengeCompletions.Add(new ChallengeCompletion
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ChallengeTemplateId = template.Id,
            CompletedDateUtc = Today,
            CompletedAtUtc = _dateTimeProvider.UtcNow,
            XpAwarded = template.XpReward,
            CoinsAwarded = template.CoinReward,
        });
        _db.UserProgresses.Add(new UserProgress
        {
            UserId = userId,
            Xp = template.XpReward,
            Coins = template.CoinReward,
            CurrentStreakDays = 1,
            LongestStreakDays = 1,
            LastActivityDateUtc = Today,
        });
        await _db.SaveChangesAsync();

        var result = await CreateHandler().Handle(new GetDashboardQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dashboard = result.Value;

        dashboard.Xp.Should().Be(template.XpReward);
        dashboard.TodaysChallenges.Single(c => c.Id == template.Id).IsCompleted.Should().BeTrue();
        dashboard.WeeklyActivity[^1].XpEarned.Should().Be(template.XpReward);
        dashboard.Streak.StudiedToday.Should().BeTrue();
        dashboard.Streak.CurrentDays.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithOtherUsersHavingMoreXp_ComputesMyRankAndTopCorrectly()
    {
        var userId = SeedUser("Me");
        _db.UserProgresses.Add(new UserProgress { UserId = userId, Xp = 10 });

        var leaderId = SeedUser("LeaderOfThePack");
        _db.UserProgresses.Add(new UserProgress { UserId = leaderId, Xp = 500 });

        await _db.SaveChangesAsync();

        var result = await CreateHandler().Handle(new GetDashboardQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dashboard = result.Value;

        dashboard.Leaderboard.MyRank.Should().Be(2);
        dashboard.Leaderboard.Top.Should().HaveCount(2);
        dashboard.Leaderboard.Top[0].UserId.Should().Be(leaderId);
        dashboard.Leaderboard.Top[0].Rank.Should().Be(1);
        dashboard.Leaderboard.Top[1].UserId.Should().Be(userId);
        dashboard.Leaderboard.Top[1].Rank.Should().Be(2);
    }
}
