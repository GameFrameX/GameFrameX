using System.Collections.Concurrent;
using Server.DBServer.State;

namespace Server.Cache.Memory;

public sealed class MemoryCacheService : ICache
{
    private readonly ConcurrentDictionary<string, CacheEntry> cache = new ConcurrentDictionary<string, CacheEntry>();

    public async Task SetAsync(long key, CacheState value)
    {
        cache[key.ToString()] = new CacheEntry { Value = value, Key = value.Id.ToString() };
        await Task.CompletedTask;
    }

    public async Task SetAsync(string key, CacheState value)
    {
        cache[key] = new CacheEntry { Value = value, Key = value.Id.ToString() };
        await Task.CompletedTask;
    }

    public async Task<CacheState> GetAsync(string key)
    {
        if (cache.TryGetValue(key, out var entry))
        {
            return entry.Value;
        }

        return null;
    }

    public Task<bool> TryGetAsync(string key, out CacheState value)
    {
        throw new NotImplementedException();
    }

    // public async Task<bool> TryGetAsync(string key, out CacheState value)
    // {
    //     value = null;
    //     return await GetAsync(key) is { } && _cache.TryGetValue(key, out value);
    // }

    public async Task RemoveAsync(string key)
    {
        cache.TryRemove(key, out _);
        await Task.CompletedTask;
    }

    public async Task<bool> ContainsAsync(string key)
    {
        return cache.ContainsKey(key);
    }

    public async Task FlushAsync()
    {
        cache.Clear();

        await Task.CompletedTask;
    }

    public async Task RefreshAsync(string key)
    {
        // 实现刷新逻辑，例如更新过期时间

        await Task.CompletedTask;
    }

    public Task<CacheState> GetFirstAsync()
    {
        return Task.FromResult(cache.Values.First().Value);
    }

    public bool Remove(CacheState value)
    {
        return cache.TryRemove(value.Id.ToString(), out _);
    }

    // 同步方法实现
    public void Set(string key, CacheState value)
    {
        cache[key] = new CacheEntry { Value = value };
    }

    public object Get(string key)
    {
        if (cache.TryGetValue(key, out var entry))
        {
            return entry.Value;
        }

        return null;
    }

    public bool TryGet(string key, out CacheState value)
    {
        throw new NotImplementedException();
    }

    // public bool TryGet(string key, out CacheState value)
    // {
    //     return Get(key) is { } && _cache.TryGetValue(key, out value);
    // }

    public void Remove(string key)
    {
        cache.TryRemove(key, out _);
    }

    public bool Contains(string key)
    {
        return cache.ContainsKey(key);
    }

    public void Flush()
    {
        cache.Clear();
    }

    public void Refresh(string key)
    {
        // 实现刷新逻辑，例如更新过期时间
    }
}