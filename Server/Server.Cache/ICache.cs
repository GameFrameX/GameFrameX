using Server.DBServer.State;

namespace Server.Cache;

public interface ICache
{
    // 同步方法
    void Set(string key, CacheState value);
    object Get(string key);
    bool TryGet(string key, out CacheState value);
    void Remove(string key);
    bool Contains(string key);
    void Flush(); // 清除所有缓存
    void Refresh(string key); // 刷新过期策略，而不移除缓存

    // 异步方法
    Task SetAsync(long key, CacheState value);
    Task SetAsync(string key, CacheState value);
    Task<CacheState> GetAsync(string key);
    Task<bool> TryGetAsync(string key, out CacheState value);
    Task RemoveAsync(string key);
    Task<bool> ContainsAsync(string key);
    Task FlushAsync();
    Task RefreshAsync(string key);

    Task<CacheState> GetFirstAsync();
    
    bool Remove(CacheState value);
}