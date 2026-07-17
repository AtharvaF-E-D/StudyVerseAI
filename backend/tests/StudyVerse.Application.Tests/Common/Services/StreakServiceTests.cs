using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Services;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Common.Services;

public sealed class StreakServiceTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private StreakService CreateService() => new(_db, _dateTimeProvider);

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
    public async Task RecordActivityAsync_FirstEverActivity_SetsStreakToOne()
    {
        var userId = SeedUser();

        await CreateService().RecordActivityAsync(userId, CancellationToken.None);

        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.CurrentStreakDays.Should().Be(1);
        progress.LongestStreakDays.Should().Be(1);
        progress.LastActivityDateUtc.Should().Be(DateOnly.FromDateTime(_dateTimeProvider.UtcNow));
    }

    [Fact]
    public async Task RecordActivityAsync_OnTheNextConsecutiveDay_IncrementsTheStreak()
    {
        var userId = SeedUser();
        await CreateService().RecordActivityAsync(userId, CancellationToken.None);

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(1);
        await CreateService().RecordActivityAsync(userId, CancellationToken.None);

        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.CurrentStreakDays.Should().Be(2);
        progress.LongestStreakDays.Should().Be(2);
    }

    [Fact]
    public async Task RecordActivityAsync_CalledTwiceOnTheSameDay_DoesNotDoubleIncrement()
    {
        var userId = SeedUser();
        var service = CreateService();

        await service.RecordActivityAsync(userId, CancellationToken.None);
        await service.RecordActivityAsync(userId, CancellationToken.None);

        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.CurrentStreakDays.Should().Be(1);
    }

    [Fact]
    public async Task RecordActivityAsync_AfterAGapOfTwoOrMoreDays_ResetsTheStreakToOneButKeepsTheLongest()
    {
        var userId = SeedUser();
        await CreateService().RecordActivityAsync(userId, CancellationToken.None);

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(1);
        await CreateService().RecordActivityAsync(userId, CancellationToken.None);
        // Streak is now 2 (the longest so far).

        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(3);
        await CreateService().RecordActivityAsync(userId, CancellationToken.None);

        var progress = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progress.CurrentStreakDays.Should().Be(1);
        progress.LongestStreakDays.Should().Be(2);
    }
}
