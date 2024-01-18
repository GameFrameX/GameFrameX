namespace Server.DBServer.NoSql;

public interface INoSqlHelper
{
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="connectionStrings">链接字符串</param>
    void Init(params string[] connectionStrings);

    /// <summary>
    /// 同步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    void Set(string key, string value);

    /// <summary>
    /// 同步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <typeparam name="T"></typeparam>
    void Set<T>(string key, T value) where T : class;

    /// <summary>
    /// 同步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <param name="timeOut">过期时间</param>
    /// <typeparam name="T"></typeparam>
    void Set<T>(string key, T value, TimeSpan timeOut) where T : class;

    /// <summary>
    /// 同步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <param name="timeoutSeconds">过期时间</param>
    /// <typeparam name="T"></typeparam>
    void Set<T>(string key, T value, int timeoutSeconds) where T : class;

    /// <summary>
    /// 异步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    Task SetAsync(string key, string value);

    /// <summary>
    /// 异步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task SetAsync<T>(string key, T value) where T : class;

    /// <summary>
    /// 异步设置指定的Key的数据到NoSql中，指定的过期时间
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">值</param>
    /// <param name="timeOut">过期时间</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task SetAsync<T>(string key, T value, TimeSpan timeOut) where T : class;

    /// <summary>
    /// 异步设置指定的Key的数据到NoSql中
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">值</param>
    /// <param name="timeoutSeconds">过期时间，单位秒</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task SetAsync<T>(string key, T value, int timeoutSeconds) where T : class;

    /// <summary>
    /// 从NoSql中获取指定的key的值
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    string GetString(string key);

    /// <summary>
    /// 异步从NoSql中获取指定的key的值
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    Task<string> GetStringAsync(string key);

    /// <summary>
    /// 从NoSql中获取指定的key的值，如果不存在则返回null
    /// </summary>
    /// <param name="key">Key</param>
    /// <typeparam name="T">类型</typeparam>
    /// <returns></returns>
    T Get<T>(string key) where T : class;

    /// <summary>
    /// 异步从NoSql中获取指定的key的值，如果不存在则返回null
    /// </summary>
    /// <param name="key">Key</param>
    /// <typeparam name="T">类型</typeparam>
    /// <returns></returns>
    Task<T> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// 从NoSql中删除指定的keys，如果不存在则忽略
    /// </summary>
    /// <param name="keys">Keys列表</param>
    /// <returns></returns>
    long Delete(params string[] keys);

    /// <summary>
    /// 从NoSql中删除指定的keys，如果不存在则忽略
    /// </summary>
    /// <param name="keys">Keys列表</param>
    /// <returns></returns>
    Task<long> DeleteAsync(params string[] keys);

    /// <summary>
    /// 判断指定的key是否存在
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    long Exists(params string[] keys);

    /// <summary>
    /// 异步判断指定的key是否存在
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    Task<long> ExistsAsync(params string[] keys);

    /// <summary>
    /// 判断指定的key是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool Exists(string key);

    /// <summary>
    /// 异步判断指定的key是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// 从NoSql中设置指定Key的过期时间
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="seconds">过期时间</param>
    /// <returns></returns>
    bool Expire(string key, int seconds);

    /// <summary>
    /// 异步从NoSql中设置指定Key的过期时间
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="seconds">过期时间</param>
    /// <returns></returns>
    Task<bool> ExpireAsync(string key, int seconds);

    /// <summary>
    /// 从NoSql中设置指定Key的过期时间
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="expireTime">过期时间</param>
    /// <returns></returns>
    bool Expire(string key, TimeSpan expireTime);

    /// <summary>
    /// 异步从NoSql中设置指定Key的过期时间
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="expireTime">过期时间</param>
    /// <returns></returns>
    Task<bool> ExpireAsync(string key, TimeSpan expireTime);

    /// <summary>
    /// 从NoSql中删除指定Key的过期时间
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    bool RemoveExpireTime(string key);

    /// <summary>
    /// 异步从NoSql中删除指定Key的过期时间
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    Task<bool> RemoveExpireTimeAsync(string key);
}