namespace StudyVerse.Domain.Gamification;

/// <summary>
/// Maps a random roll into a prize from <see cref="SpinPrizeCatalog"/>, using cumulative-weight
/// bucketing: the roll must be a uniformly-distributed integer in
/// [0, <see cref="SpinPrizeCatalog.TotalWeight"/>) - the caller (<c>SpinCommandHandler</c>, via
/// <c>IRandomProvider</c>) is responsible for producing that roll so this stays a pure, easily
/// unit-testable function of its input.
/// </summary>
public static class SpinWheelSelector
{
    public static SpinPrize SelectPrize(int roll)
    {
        var cumulative = 0;
        foreach (var prize in SpinPrizeCatalog.All)
        {
            cumulative += prize.Weight;
            if (roll < cumulative)
            {
                return prize;
            }
        }

        // Defensive fallback only reachable if roll >= TotalWeight (caller contract violation).
        return SpinPrizeCatalog.All[^1];
    }
}
