using Server.DBServer.State;

namespace Server.Cache.Memory;

public sealed class MemoryCacheService : ICache
{
    private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();

    public async Task SetAsync(string key, CacheState value)
    {
        _cache[key] = new CacheEntry { Value = value };
    }

    public async Task<CacheState> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
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
        _cache.Remove(key);
    }

    public async Task<bool> ContainsAsync(string key)
    {
        return _cache.ContainsKey(key);
    }

    public async Task FlushAsync()
    {
        _cache.Clear();
    }

    public async Task RefreshAsync(string key)
    {
        // 实现刷新逻辑，例如更新过期时间
    }

    // 同步方法实现
    public void Set(string key, CacheState value)
    {
        _cache[key] = new CacheEntry { Value = value };
    }

    public object Get(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
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
        _cache.Remove(key);
    }

    public bool Contains(string key)
    {
        return _cache.ContainsKey(key);
    }

    public void Flush()
    {
        _cache.Clear();
    }

    public void Refresh(string key)
    {
        // 实现刷新逻辑，例如更新过期时间
    }
}