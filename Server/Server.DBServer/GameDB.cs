/*using System.Linq.Expressions;
using Server.DBServer.State;

namespace Server.DBServer
{
    /// <summary>
    /// 游戏数据库类
    /// </summary>
    public static class GameDb
    {
        private static IGameDbService _dbServiceImpler;

        /// <summary>
        /// 使用指定的dbService初始化GameDb
        /// </summary>
        /// <param name="dbService">dbService的实现</param>
        public static void Init(IGameDbService dbService)
        {
            _dbServiceImpler = dbService;
        }

        /// <summary>
        /// 以指定类型获取GameDb
        /// </summary>
        /// <typeparam name="T">dbService的类型</typeparam>
        /// <returns>以指定类型返回的GameDb</returns>
        public static T As<T>() where T : IGameDbService
        {
            return (T)_dbServiceImpler;
        }

        /// <summary>
        /// 使用指定的mongoUrl和mongoDbName打开GameDb连接
        /// </summary>
        /// <param name="mongoUrl">MongoDB连接URL</param>
        /// <param name="mongoDbName">MongoDB数据库的名称</param>
        public static void Open(string mongoUrl, string mongoDbName)
        {
            _dbServiceImpler.Open(mongoUrl, mongoDbName);
        }

        /// <summary>
        /// 关闭GameDb连接
        /// </summary>
        public static void Close()
        {
            _dbServiceImpler.Close();
        }

        /// <summary>
        /// 查找与指定过滤器匹配的文档列表
        /// </summary>
        /// <typeparam name="TState">文档的类型</typeparam>
        /// <param name="filter">过滤器表达式</param>
        /// <returns>表示异步操作的任务。任务结果包含文档列表</returns>
        public static Task<List<TState>> FindListAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new()
        {
            return _dbServiceImpler.FindListAsync<TState>(filter);
        }

        /// <summary>
        /// 计算与指定过滤器匹配的文档数量
        /// </summary>
        /// <typeparam name="TState">文档的类型</typeparam>
        /// <param name="filter">过滤器表达式</param>
        /// <returns>表示异步操作的任务。任务结果包含文档数量</returns>
        public static Task<long> CountAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new()
        {
            return _dbServiceImpler.CountAsync<TState>(filter);
        }

        /// <summary>
        /// 查找与指定过滤器匹配的文档
        /// </summary>
        /// <typeparam name="TState">文档的类型</typeparam>
        /// <param name="filter">过滤器表达式</param>
        /// <returns>表示异步操作的任务。任务结果包含文档</returns>
        public static Task<TState> FindAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new()
        {
            return _dbServiceImpler.FindAsync<TState>(filter);
        }

        /// <summary>
        /// 加载指定id的文档
        /// </summary>
        /// <typeparam name="TState">文档的类型</typeparam>
        /// <param name="id">文档的id</param>
        /// <param name="defaultGetter">一个用于获取默认值的函数，如果指定的文档不存在</param>
        /// <returns>表示异步操作的任务。任务结果包含文档</returns>
        public static Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : ICacheState, new()
        {
            return _dbServiceImpler.LoadState(id, defaultGetter);
        }

        /// <summary>
        /// 异步更新指定类型的文档
        /// </summary>
        /// <typeparam name="TState">文档的类型</typeparam>
        /// <param name="state">要更新的文档</param>
        /// <returns>表示异步操作的任务。任务结果包含更新后的文档</returns>
        public static Task<TState> UpdateAsync<TState>(TState state) where TState : ICacheState, new()
        {
            return _dbServiceImpler.UpdateAsync<TState>(state);
        }

        /// <summary>
        /// 异步保存一个文档
        /// </summary>
        /// <typeparam name="TState">文档的类型</typeparam>
        /// <param name="state">要保存的文档</param>
        /// <returns>表示异步操作的任务</returns>
        public static Task SaveOneAsync<TState>(TState state) where TState : CacheState, new()
        {
            return _dbServiceImpler.AddAsync<TState>(state);
        }

        /// <summary>
        /// 异步删除与指定过滤器匹配的文档
        /// </summary>
        /// <typeparam name="TState">文档的类型</typeparam>
        /// <param name="filter">过滤器表达式</param>
        /// <returns>表示异步操作的任务。任务结果包含删除的文档数量</returns>
        public static Task<long> DeleteAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new()
        {
            return _dbServiceImpler.DeleteAsync<TState>(filter);
        }

        /// <summary>
        /// 异步删除指定的文档
        /// </summary>
        /// <typeparam name="TState">文档的类型</typeparam>
        /// <param name="state">要删除的文档</param>
        /// <returns>表示异步操作的任务。任务结果包含删除的文档数量</returns>
        public static Task<long> DeleteAsync<TState>(TState state) where TState : ICacheState, new()
        {
            return _dbServiceImpler.DeleteAsync<TState>(state);
        }

        public static async Task SaveAll()
        {
            // if (GlobalSettings.DBModel == (int) DbModel.Embeded)
            // {
            //     await ActorMgr.SaveAll();
            // }
            // else if (GlobalSettings.DBModel == (int) DbModel.Mongodb)
            // {
            //     await StateComp.SaveAll();
            // }
        }

        public static async Task TimerSave()
        {
            // if (GlobalSettings.DBModel == (int) DbModel.Embeded)
            // {
            //     await ActorMgr.TimerSave();
            // }
            // else if (GlobalSettings.DBModel == (int) DbModel.Mongodb)
            // {
            //     await StateComp.TimerSave();
            // }
        }
    }
}*/