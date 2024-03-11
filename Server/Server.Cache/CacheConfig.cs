namespace Server.Cache;

public class CacheConfig
{
    public int CacheSize { get; set; }
    public TimeSpan DefaultExpiration { get; set; }
    public string RedisConnectionString { get; set; }
}