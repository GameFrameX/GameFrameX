using System.Linq.Expressions;
using Server.DBServer.State;

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
    public Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : ICacheState, new();

    /// <summary>
    /// 查询单条数据
    /// </summary>
    /// <param name="filter">查询条件</param>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public Task<TState> FindAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new();

    /// <summary>
    /// 查询数据
    /// </summary>
    /// <param name="filter">查询条件</param>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public Task<List<TState>> FindListAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new();


    /// <summary>
    /// 查询数据长度
    /// </summary>
    /// <param name="filter">查询条件</param>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public Task<long> CountAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new();

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="filter">查询条件</param>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public Task<long> DeleteAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new();

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="state"></param>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public Task<long> DeleteAsync<TState>(TState state) where TState : ICacheState, new();

    /// <summary>
    /// 保存数据
    /// </summary>
    /// <param name="state"></param>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public Task<long> AddAsync<TState>(TState state) where TState : ICacheState, new();

    /// <summary>
    /// 保存数据
    /// </summary>
    /// <param name="state"></param>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public Task<TState> UpdateAsync<TState>(TState state) where TState : ICacheState, new();
}