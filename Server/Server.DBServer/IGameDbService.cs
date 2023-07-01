using Server.DBServer.Storage;

namespace Server.DBServer;

/// <summary>
/// 数据库服务
/// </summary>
public interface IGameDbService
{
    /// <summary>
    /// 链接数据库
    /// </summary>
    /// <param name="url">链接地址</param>
    /// <param name="dbName">数据库名称</param>
    public void Open(string url, string dbName);

    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public void Close();

    /// <summary>
    /// 加载数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="defaultGetter"></param>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new();

    /// <summary>
    /// 保存数据
    /// </summary>
    /// <param name="state"></param>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public Task SaveState<TState>(TState state) where TState : CacheState;
}