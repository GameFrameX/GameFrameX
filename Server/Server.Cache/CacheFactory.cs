namespace Server.Cache;

public static class CacheManager
{
    public static ICache Instance
    {
        get
        {
            if (_cache == null)
            {
                throw new ArgumentNullException(nameof(_cache), "请先调用Server.Cache.CacheFactory.Init(_cache)初始化缓存服务");
            }

            return _cache;
        }
    }

    private static ICache? _cache;

    public static ICache Init(ICache cache)
    {
        _cache = cache;
        return _cache;
    }
}