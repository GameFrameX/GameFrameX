using Server.DBServer.State;

namespace Server.Cache;

public class CacheEntry
{
    public string Key { get; set; }

    public CacheState Value { get; set; }
    // 可以添加更多属性，例如过期时间等
}