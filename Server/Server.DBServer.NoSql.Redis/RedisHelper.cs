using FreeRedis;
using Server.Utility;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Server.DBServer.NoSql.Redis;

public partial class RedisHelper : INoSqlHelper
{
    private RedisClient client;


    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="connectionStrings">链接字符串</param>
    public void Init(params string[] connectionStrings)
    {
        ConnectionStringBuilder[] connectionStringBuilders = new ConnectionStringBuilder[connectionStrings.Length];
        for (var index = 0; index < connectionStrings.Length; index++)
        {
            var connectionString = connectionStrings[index];
            connectionStringBuilders[index] = connectionString;
        }

        client = new RedisClient(connectionStringBuilders);
    }

    /// <summary>
    /// 从NoSql中删除指定的keys，如果不存在则忽略
    /// </summary>
    /// <param name="keys">Keys列表</param>
    /// <returns></returns>
    public long Delete(params string[] keys)
    {
        return client.Del(keys);
    }

    /// <summary>
    /// 从NoSql中删除指定的keys，如果不存在则忽略
    /// </summary>
    /// <param name="keys">Keys列表</param>
    /// <returns></returns>
    public Task<long> DeleteAsync(params string[] keys)
    {
        return client.DelAsync(keys);
    }

    /// <summary>
    /// 判断指定的key是否存在
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public long Exists(params string[] keys)
    {
        Guard.NotNull(client, nameof(client));
        return client.Exists(keys);
    }

    /// <summary>
    /// 异步判断指定的key是否存在
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public Task<long> ExistsAsync(params string[] keys)
    {
        Guard.NotNull(client, nameof(client));
        return client.ExistsAsync(keys);
    }

    /// <summary>
    /// 判断指定的key是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Exists(string key)
    {
        NullGuard(key);
        return client.Exists(key);
    }

    /// <summary>
    /// 异步判断指定的key是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task<bool> ExistsAsync(string key)
    {
        NullGuard(key);
        return client.ExistsAsync(key);
    }

    /// <summary>
    /// 从NoSql中设置指定Key的过期时间
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="seconds">过期时间</param>
    /// <returns></returns>
    public bool Expire(string key, int seconds)
    {
        NullGuard(key);
        return client.Expire(key, seconds);
    }

    /// <summary>
    /// 异步从NoSql中设置指定Key的过期时间
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="seconds">过期时间</param>
    /// <returns></returns>
    public Task<bool> ExpireAsync(string key, int seconds)
    {
        NullGuard(key);
        return client.ExpireAsync(key, seconds);
    }

    /// <summary>
    /// 从NoSql中设置指定Key的过期时间
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="expireTime">过期时间</param>
    /// <returns></returns>
    public bool Expire(string key, TimeSpan expireTime)
    {
        NullGuard(key);
        return client.Expire(key, expireTime);
    }

    /// <summary>
    /// 异步从NoSql中设置指定Key的过期时间
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="expireTime">过期时间</param>
    /// <returns></returns>
    public Task<bool> ExpireAsync(string key, TimeSpan expireTime)
    {
        NullGuard(key);
        return client.ExpireAsync(key, expireTime);
    }

    /// <summary>
    /// 从NoSql中删除指定Key的过期时间
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    public bool RemoveExpireTime(string key)
    {
        NullGuard(key);
        return client.Persist(key);
    }

    /// <summary>
    /// 异步从NoSql中删除指定Key的过期时间
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    public Task<bool> RemoveExpireTimeAsync(string key)
    {
        NullGuard(key);
        return client.PersistAsync(key);
    }

    private void NullGuard(string key)
    {
        Guard.NotNull(client, nameof(client));
        Guard.NotNullOrEmpty(key, nameof(key));
    }

    private void NullGuard<T>(string key, T value) where T : class
    {
        NullGuard(key);
        Guard.NotNull(value, nameof(value));
    }
}