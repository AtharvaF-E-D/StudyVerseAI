using StudyVerse.Application.Common.Interfaces;

namespace StudyVerse.Infrastructure.Common;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
