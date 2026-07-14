using StudyVerse.Application.Common.Interfaces;

namespace StudyVerse.Application.Tests.TestSupport;

/// <summary>A controllable clock so tests can assert exact expiry/lockout instants and simulate time passing.</summary>
public sealed class TestDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
