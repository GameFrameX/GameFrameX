using FreeRedis;
using Server.Utility;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Server.DBServer.NoSql.Redis;

public partial  class RedisHelper : INoSqlHelper
{
   

    /// <summary>
    /// 同步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    public void Set(string key, string value)
    {
        NullGuard(key, value);
        client.Set(key, value);
    }

    /// <summary>
    /// 同步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <typeparam name="T"></typeparam>
    public void Set<T>(string key, T value) where T : class
    {
        NullGuard(key, value);
        client.Set(key, value);
    }

    /// <summary>
    /// 同步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <param name="timeOut">过期时间</param>
    /// <typeparam name="T"></typeparam>
    public void Set<T>(string key, T value, TimeSpan timeOut) where T : class
    {
        NullGuard(key, value);
        client.Set(key, value, timeOut);
    }

    /// <summary>
    /// 同步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <param name="timeoutSeconds">过期时间</param>
    /// <typeparam name="T"></typeparam>
    public void Set<T>(string key, T value, int timeoutSeconds) where T : class
    {
        NullGuard(key, value);
        client.Set(key, value, timeoutSeconds);
    }

    /// <summary>
    /// 异步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public Task SetAsync(string key, string value)
    {
        NullGuard(key, value);
        return client.SetAsync(key, value);
    }

    /// <summary>
    /// 异步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task SetAsync<T>(string key, T value) where T : class
    {
        NullGuard(key, value);
        return client.SetAsync(key, value);
    }

    /// <summary>
    /// 异步设置指定的Key的数据到NoSql中，指定的过期时间
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">值</param>
    /// <param name="timeOut">过期时间</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task SetAsync<T>(string key, T value, TimeSpan timeOut) where T : class
    {
        NullGuard(key, value);
        return client.SetAsync(key, value, timeOut);
    }

    /// <summary>
    /// 异步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">值</param>
    /// <param name="timeoutSeconds">过期时间，单位秒</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task SetAsync<T>(string key, T value, int timeoutSeconds) where T : class
    {
        NullGuard(key, value);
        return client.SetAsync(key, value, timeoutSeconds);
    }
}