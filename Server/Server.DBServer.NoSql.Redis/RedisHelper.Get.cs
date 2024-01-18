using Server.Utility;

namespace Server.DBServer.NoSql.Redis;

public partial class RedisHelper : INoSqlHelper
{
    /// <summary>
    /// 从NoSql中获取指定的key的值
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    public string GetString(string key)
    {
        NullGuard(key);
        return client.Get<string>(key);
    }

    /// <summary>
    /// 异步从NoSql中获取指定的key的值
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    public Task<string> GetStringAsync(string key)
    {
        NullGuard(key);
        return client.GetAsync<string>(key);
    }

    /// <summary>
    /// 从NoSql中获取指定的key的值，如果不存在则返回null
    /// </summary>
    /// <param name="key">Key</param>
    /// <typeparam name="T">类型</typeparam>
    /// <returns></returns>
    public T Get<T>(string key) where T : class
    {
        Guard.NotNull(client, nameof(client));
        Guard.NotNullOrEmpty(key, nameof(key));
        return client.Get<T>(key);
    }

    /// <summary>
    /// 异步从NoSql中获取指定的key的值，如果不存在则返回null
    /// </summary>
    /// <param name="key">Key</param>
    /// <typeparam name="T">类型</typeparam>
    /// <returns></returns>
    public Task<T> GetAsync<T>(string key) where T : class
    {
        Guard.NotNull(client, nameof(client));
        Guard.NotNullOrEmpty(key, nameof(key));
        return client.GetAsync<T>(key);
    }
}