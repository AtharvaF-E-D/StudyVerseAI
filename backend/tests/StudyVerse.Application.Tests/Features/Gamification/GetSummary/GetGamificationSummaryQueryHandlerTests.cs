using FluentAssertions;
using StudyVerse.Application.Features.Gamification.Common;
using StudyVerse.Application.Features.Gamification.GetSummary;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Features.Gamification.GetSummary;

public sealed class GetGamificationSummaryQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetGamificationSummaryQueryHandler CreateHandler() => new(
        _db,
        _dateTimeProvider,
        new BadgeEvaluationService(_db, _dateTimeProvider),
        new MissionProgressService(_db, _dateTimeProvider));

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
        var result = await CreateHandler().Handle(new GetGamificationSummaryQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ForABrandNewUser_ReturnsZeroedOutDefaultsAndTheFullBadgeMissionCatalogCounts()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetGamificationSummaryQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Xp.Should().Be(0);
        result.Value.Coins.Should().Be(0);
        result.Value.Level.Should().Be(LevelCalculator.GetLevel(0));
        result.Value.CurrentStreakDays.Should().Be(0);
        result.Value.BadgesEarnedCount.Should().Be(0);
        result.Value.TotalBadgesCount.Should().Be(BadgeCatalog.All.Count);
        result.Value.MissionsCompletedThisWeek.Should().Be(0);
        result.Value.TotalMissionsThisWeek.Should().Be(3);
        result.Value.DailyRewardStatus.ClaimedToday.Should().BeFalse();
        result.Value.SpinStatus.SpunToday.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithRealFlashcardActivityAndAnExistingProgressRow_ReflectsBothInTheSummary()
    {
        var userId = SeedUser();
        _db.UserProgresses.Add(new UserProgress { UserId = userId, Xp = 120, Coins = 40, CurrentStreakDays = 4, LongestStreakDays = 4 });
        _db.FlashcardDecks.Add(new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Deck",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetGamificationSummaryQuery(userId), CancellationToken.None);

        result.Value.Xp.Should().Be(120);
        result.Value.Coins.Should().Be(40);
        result.Value.Level.Should().Be(LevelCalculator.GetLevel(120));
        result.Value.CurrentStreakDays.Should().Be(4);
        result.Value.BadgesEarnedCount.Should().Be(1); // Bookworm, just evaluated lazily
    }
}
