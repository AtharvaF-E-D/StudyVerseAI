namespace StudyVerse.Domain.Gamification;

/// <summary>One weighted outcome on the daily spin wheel.</summary>
public sealed record SpinPrize(string Label, int Weight, int CoinsAwarded, int XpAwarded);

/// <summary>
/// The fixed, weighted daily spin-wheel prize table. Weights are relative (out of
/// <see cref="TotalWeight"/>, currently 100, chosen so each weight also reads directly as a
/// percentage) - decreasing as the coin payout increases, one dedicated "bonus XP" outcome, one
/// rare jackpot, and one small "better luck tomorrow" outcome so not every spin pays out currency.
/// See <see cref="SpinWheelSelector"/> for how a random roll maps to one of these.
/// </summary>
public static class SpinPrizeCatalog
{
    public static readonly IReadOnlyList<SpinPrize> All =
    [
        new("10 Coins", Weight: 30, CoinsAwarded: 10, XpAwarded: 0),
        new("20 Coins", Weight: 25, CoinsAwarded: 20, XpAwarded: 0),
        new("50 Coins", Weight: 18, CoinsAwarded: 50, XpAwarded: 0),
        new("Bonus XP", Weight: 12, CoinsAwarded: 0, XpAwarded: 25),
        new("100 Coins", Weight: 8, CoinsAwarded: 100, XpAwarded: 0),
        new("Better Luck Tomorrow", Weight: 4, CoinsAwarded: 0, XpAwarded: 0),
        new("200 Coins", Weight: 2, CoinsAwarded: 200, XpAwarded: 0),
        new("Jackpot! 500 Coins + 50 XP", Weight: 1, CoinsAwarded: 500, XpAwarded: 50),
    ];

    public static readonly int TotalWeight = All.Sum(p => p.Weight);
}
