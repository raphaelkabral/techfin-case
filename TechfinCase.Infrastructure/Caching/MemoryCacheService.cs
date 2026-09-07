using Microsoft.Extensions.Caching.Memory;
using TechfinCase.Application.Abstractions;

namespace TechfinCase.Infrastructure.Caching;

public sealed class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    public bool TryGet<T>(string key, out T? value) =>
        cache.TryGetValue(key, out value);

    public void Set<T>(string key, T value, TimeSpan expiration) =>
        cache.Set(key, value, expiration);

    public void Remove(string key) => cache.Remove(key);
}
