using StudyVerse.Application.Common.Interfaces;

namespace StudyVerse.Application.Tests.TestSupport;

/// <summary>A trivial in-memory <see cref="ICacheService"/> fake — no TTL expiry, good enough for
/// tests that only care whether a key is present/absent.</summary>
public sealed class TestCacheService : ICacheService
{
    private readonly Dictionary<string, string> _store = new();

    public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.TryGetValue(key, out var value) ? value : null);

    public Task SetAsync(string key, string value, TimeSpan timeToLive, CancellationToken cancellationToken = default)
    {
        _store[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.ContainsKey(key));
}
