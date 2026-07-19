namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Abstraction over randomness so the spin wheel's outcome is deterministic and mockable in tests
/// (the same "wrap the non-deterministic thing behind an interface" reasoning as
/// <see cref="IDateTimeProvider"/> for the clock). The real implementation wraps <c>Random.Shared</c>.
/// </summary>
public interface IRandomProvider
{
    /// <summary>Returns a random integer in [<paramref name="minValueInclusive"/>, <paramref name="maxValueExclusive"/>).</summary>
    int Next(int minValueInclusive, int maxValueExclusive);
}
