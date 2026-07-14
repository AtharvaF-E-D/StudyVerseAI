namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Thin string key/value cache abstraction (Redis-backed in Infrastructure) used for rate
/// limiting, account lockout state, and OTP-request throttling.
/// </summary>
public interface ICacheService
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SetAsync(string key, string value, TimeSpan timeToLive, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
