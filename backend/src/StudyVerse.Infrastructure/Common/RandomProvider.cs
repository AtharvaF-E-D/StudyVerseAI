using StudyVerse.Application.Common.Interfaces;

namespace StudyVerse.Infrastructure.Common;

public sealed class RandomProvider : IRandomProvider
{
    public int Next(int minValueInclusive, int maxValueExclusive) => Random.Shared.Next(minValueInclusive, maxValueExclusive);
}
