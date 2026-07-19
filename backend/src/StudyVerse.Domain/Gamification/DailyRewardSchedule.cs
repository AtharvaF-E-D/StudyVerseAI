namespace StudyVerse.Domain.Gamification;

/// <summary>
/// The fixed, escalating 7-day daily-login-reward schedule. Coins escalate every day; XP kicks in
/// from day 3 onward as a bigger "you've kept the streak going" bump, culminating in the day-7
/// reward before the cycle wraps back to day 1. See <see cref="Entities.DailyRewardClaim"/> for how
/// <see cref="Entities.DailyRewardClaim.ConsecutiveDayNumber"/> advances/resets.
///
/// Schedule (day: coins / xp): 1: 10/0, 2: 15/0, 3: 20/5, 4: 25/5, 5: 30/10, 6: 40/10, 7: 50/20.
/// </summary>
public static class DailyRewardSchedule
{
    public const int CycleLength = 7;

    private static readonly int[] CoinsByDay = [10, 15, 20, 25, 30, 40, 50];
    private static readonly int[] XpByDay = [0, 0, 5, 5, 10, 10, 20];

    public static (int Coins, int Xp) GetReward(int dayNumber)
    {
        if (dayNumber is < 1 or > CycleLength)
        {
            throw new ArgumentOutOfRangeException(nameof(dayNumber), dayNumber, $"Day number must be between 1 and {CycleLength}.");
        }

        return (CoinsByDay[dayNumber - 1], XpByDay[dayNumber - 1]);
    }

    /// <summary>The day number after <paramref name="dayNumber"/>, cycling back to 1 after <see cref="CycleLength"/>.</summary>
    public static int NextDayNumber(int dayNumber) => dayNumber >= CycleLength ? 1 : dayNumber + 1;
}
