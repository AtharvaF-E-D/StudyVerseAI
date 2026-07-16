using Microsoft.Extensions.Caching.Memory;
using StudyVerse.Application.Common.Interfaces;

namespace StudyVerse.Infrastructure.Caching;

/// <summary>
/// Local-development fallback used when no Redis connection string is
/// configured (see <see cref="DependencyInjection.AddInfrastructure"/>).
/// Not suitable beyond a single process — OTP/lockout/rate-limit state
/// isn't shared across instances — so Staging/Production must always
/// configure a real Redis connection string.
/// </summary>
public sealed class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_cache.Get<string>(key));

    public Task SetAsync(string key, string value, TimeSpan timeToLive, CancellationToken cancellationToken = default)
    {
        _cache.Set(key, value, timeToLive);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_cache.TryGetValue(key, out _));
}
