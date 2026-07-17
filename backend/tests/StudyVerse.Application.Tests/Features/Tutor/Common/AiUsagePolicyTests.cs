using FluentAssertions;
using StudyVerse.Application.Features.Tutor.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.Tutor.Common;

public sealed class AiUsagePolicyTests
{
    private static readonly DateOnly Today = new(2026, 1, 1);
    private static readonly DateOnly Yesterday = Today.AddDays(-1);

    [Fact]
    public void ResetIfNewDay_FirstEverUsage_ResetsToZeroAndStampsToday()
    {
        var progress = new UserProgress { UserId = Guid.NewGuid(), AiTokensUsedToday = 0, AiUsageResetDateUtc = null };

        AiUsagePolicy.ResetIfNewDay(progress, Today);

        progress.AiTokensUsedToday.Should().Be(0);
        progress.AiUsageResetDateUtc.Should().Be(Today);
    }

    [Fact]
    public void ResetIfNewDay_WhenTheStoredResetDateIsToday_LeavesTheCounterUntouched()
    {
        var progress = new UserProgress { UserId = Guid.NewGuid(), AiTokensUsedToday = 4200, AiUsageResetDateUtc = Today };

        AiUsagePolicy.ResetIfNewDay(progress, Today);

        progress.AiTokensUsedToday.Should().Be(4200);
        progress.AiUsageResetDateUtc.Should().Be(Today);
    }

    [Fact]
    public void ResetIfNewDay_WhenTheDayHasRolledOver_ResetsTheCounterAndStampsTheNewDate()
    {
        var progress = new UserProgress { UserId = Guid.NewGuid(), AiTokensUsedToday = 49_999, AiUsageResetDateUtc = Yesterday };

        AiUsagePolicy.ResetIfNewDay(progress, Today);

        progress.AiTokensUsedToday.Should().Be(0);
        progress.AiUsageResetDateUtc.Should().Be(Today);
    }

    [Fact]
    public void GetTokensUsedToday_NoProgressRowYet_ReturnsZero()
    {
        AiUsagePolicy.GetTokensUsedToday(null, Today).Should().Be(0);
    }

    [Fact]
    public void GetTokensUsedToday_StoredResetDateIsToday_ReturnsTheStoredCounterWithoutMutating()
    {
        var progress = new UserProgress { UserId = Guid.NewGuid(), AiTokensUsedToday = 1234, AiUsageResetDateUtc = Today };

        AiUsagePolicy.GetTokensUsedToday(progress, Today).Should().Be(1234);
        progress.AiTokensUsedToday.Should().Be(1234, "read-only reporting must never mutate the entity");
    }

    [Fact]
    public void GetTokensUsedToday_StoredResetDateIsStale_ReportsZeroWithoutMutating()
    {
        var progress = new UserProgress { UserId = Guid.NewGuid(), AiTokensUsedToday = 1234, AiUsageResetDateUtc = Yesterday };

        AiUsagePolicy.GetTokensUsedToday(progress, Today).Should().Be(0);
        progress.AiTokensUsedToday.Should().Be(1234, "read-only reporting must never mutate the entity");
    }
}
