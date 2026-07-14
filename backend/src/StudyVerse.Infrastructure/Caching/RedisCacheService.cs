using StackExchange.Redis;
using StudyVerse.Application.Common.Interfaces;

namespace StudyVerse.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    private IDatabase Database => _connectionMultiplexer.GetDatabase();

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await Database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public Task SetAsync(string key, string value, TimeSpan timeToLive, CancellationToken cancellationToken = default) =>
        Database.StringSetAsync(key, value, timeToLive);

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        Database.KeyDeleteAsync(key);

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        Database.KeyExistsAsync(key);
}
