using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Server.DBServer.State;
using Server.Extension;
using Server.Log;

namespace Server.DBServer.DbService.MongoDB
{
    /// <summary>
    /// MongoDB服务连接类，实现了 <see cref="IGameDbService"/> 接口。
    /// </summary>
    public class MongoDbServiceConnection : IGameDbService
    {
        /// <summary>
        /// 获取或设置MongoDB客户端。
        /// </summary>
        public MongoClient Client { get; private set; }

        /// <summary>
        /// 获取或设置当前使用的MongoDB数据库。
        /// </summary>
        public IMongoDatabase CurrentDatabase { get; private set; }

        /// <summary>
        /// 打开MongoDB连接并指定URL和数据库名称。
        /// </summary>
        /// <param name="url">MongoDB连接URL。</param>
        /// <param name="dbName">要使用的数据库名称。</param>
        public async void Open(string url, string dbName)
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(url);
                Client = new MongoClient(settings);
                CurrentDatabase = Client.GetDatabase(dbName);
                LogHelper.Info($"初始化MongoDB服务完成 Url:{url} DbName:{dbName}");
            }
            catch (Exception)
            {
                LogHelper.Error($"初始化MongoDB服务失败 Url:{url} DbName:{dbName}");
                throw;
            }
        }

        /// <summary>
        /// 获取指定类型的MongoDB集合。
        /// </summary>
        /// <typeparam name="TState">文档的类型。</typeparam>
        /// <returns>指定类型的MongoDB集合。</returns>
        private IMongoCollection<TState> GetCollection<TState>() where TState : ICacheState, new()
        {
            var collectionName = typeof(TState).Name;
            IMongoCollection<TState>? collection = CurrentDatabase.GetCollection<TState>(collectionName);
            return collection;
        }

        /// <summary>
        /// 加载指定ID的缓存状态。
        /// </summary>
        /// <typeparam name="TState">缓存状态的类型。</typeparam>
        /// <param name="id">要加载的缓存状态的ID。</param>
        /// <param name="defaultGetter">默认值获取器。</param>
        /// <returns>加载的缓存状态。</returns>
        public async Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : ICacheState, new()
        {
            var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, id);

            var col = GetCollection<TState>();
            using var cursor = await col.FindAsync(filter);
            var state = await cursor.FirstOrDefaultAsync();
            bool isNew = state == null;
            if (state == null && defaultGetter != null)
            {
                state = defaultGetter();
            }

            if (state == null)
            {
                state = new TState { Id = id };
            }

            state.AfterLoadFromDB(isNew);
            return state;
        }

        /// <summary>
        /// 获取默认的查询表达式。
        /// </summary>
        /// <typeparam name="TState">缓存状态的类型。</typeparam>
        /// <param name="filter">自定义查询表达式。</param>
        /// <returns>默认的查询表达式。</returns>
        private static Expression<Func<TState, bool>> GetDefaultFindExpression<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new()
        {
            Expression<Func<TState, bool>> expression = m => m.IsDeleted == false;
            if (filter != null)
            {
                expression = expression.And(filter);
            }

            return expression;
        }

        /// <summary>
        /// 异步查找满足指定条件的缓存状态列表。
        /// </summary>
        /// <typeparam name="TState">缓存状态的类型。</typeparam>
        /// <param name="filter">查询条件。</param>
        /// <returns>满足条件的缓存状态列表。</returns>
        public async Task<List<TState>> FindListAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new()
        {
            var result = new List<TState>();
            var collection = GetCollection<TState>();
            using var cursor = await collection.FindAsync<TState>(GetDefaultFindExpression(filter));
            while (await cursor.MoveNextAsync())
            {
                result.AddRange(cursor.Current);
            }

            return result;
        }

        /// <summary>
        /// 异步查找满足指定条件的缓存状态。
        /// </summary>
        /// <typeparam name="TState">缓存状态的类型。</typeparam>
        /// <param name="filter">查询条件。</param>
        /// <returns>满足条件的缓存状态。</returns>
        public async Task<TState> FindAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new()
        {
            var collection = GetCollection<TState>();
            var findExpression = GetDefaultFindExpression(filter);
            var filterDefinition = Builders<TState>.Filter.Where(findExpression);
            using var cursor = await collection.FindAsync<TState>(filterDefinition);
            var state = await cursor.FirstOrDefaultAsync();
            return state;
        }


        /// <summary>
        /// 查询数据长度
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public async Task<long> CountAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new()
        {
            var collection = GetCollection<TState>();
            var newFilter = GetDefaultFindExpression(filter);
            var count = await collection.CountDocumentsAsync<TState>(newFilter);
            return count;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public async Task<long> DeleteAsync<TState>(Expression<Func<TState, bool>> filter) where TState : ICacheState, new()
        {
            // var newFilter = Builders<TState>.Filter.Where(filter);
            // var collectionName = typeof(TState).Name;
            // var collection = CurrentDatabase.GetCollection<TState>(collectionName);
            // state.DeleteTime = DateTime.UtcNow;
            // state.IsDeleted = true;
            // var result = await collection.ReplaceOneAsync(filter, state, ReplaceOptions);
            // return result.ModifiedCount;
            return -1;
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        public async Task<long> DeleteAsync<TState>(TState state) where TState : ICacheState, new()
        {
            var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, state.Id);
            var collection = GetCollection<TState>();
            state.DeleteTime = Utility.TimeHelper.UnixTimeSeconds();
            state.IsDeleted = true;
            var result = await collection.ReplaceOneAsync(filter, state, ReplaceOptions);
            return result.ModifiedCount;
        }

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        public async Task<long> AddAsync<TState>(TState state) where TState : ICacheState, new()
        {
            var collection = GetCollection<TState>();
            state.CreateTime = Utility.TimeHelper.UnixTimeSeconds();
            var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, state.Id);
            var result = await collection.ReplaceOneAsync(filter, state, ReplaceOptions);
            return result.ModifiedCount;
        }

        /// <summary>
        /// 增加一个列表数据
        /// </summary>
        /// <param name="states"></param>
        /// <typeparam name="TState"></typeparam>
        public async Task AddListAsync<TState>(List<TState> states) where TState : ICacheState, new()
        {
            var collection = GetCollection<TState>();
            var cacheStates = states.ToList();
            foreach (var cacheState in cacheStates)
            {
                cacheState.CreateTime = Utility.TimeHelper.UnixTimeSeconds();
            }

            await collection.InsertManyAsync(cacheStates);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public async Task<TState> UpdateAsync<TState>(TState state) where TState : ICacheState, new()
        {
            // var (isChanged, data) = state.IsChanged();
            // if (isChanged)
            {
                // var cacheState = BsonSerializer.Deserialize<TState>(data);
                state.UpdateTime = Utility.TimeHelper.UnixTimeSeconds();
                state.UpdateCount++;
                var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, state.Id);
                var collection = GetCollection<TState>();
                var result = await collection.ReplaceOneAsync(filter, state, ReplaceOptions);
                if (result.IsAcknowledged)
                {
                    state.AfterSaveToDB();
                }
            }

            return state;
        }

        /// <summary>
        /// 替换选项，用于替换文档。设置 <see cref="IsUpsert"/> 属性为 true 可以在找不到匹配的文档时插入新文档。
        /// </summary>
        public static readonly ReplaceOptions ReplaceOptions = new() { IsUpsert = true };

        /// <summary>
        /// 批量写入选项，用于批量写入文档。设置 <see cref="IsOrdered"/> 属性为 false 可以并行执行写入操作。
        /// </summary>
        public static readonly BulkWriteOptions BulkWriteOptions = new() { IsOrdered = false };

        /// <summary>
        /// 创建指定字段的索引。
        /// </summary>
        /// <typeparam name="TState">缓存状态的类型。</typeparam>
        /// <param name="indexKey">要创建索引的字段。</param>
        /// <returns>表示异步操作的任务。</returns>
        public Task CreateIndex<TState>(string indexKey) where TState : CacheState, new()
        {
            var collection = GetCollection<TState>();
            var key = Builders<TState>.IndexKeys.Ascending(indexKey);
            var model = new CreateIndexModel<TState>(key);
            return collection.Indexes.CreateOneAsync(model);
        }

        /// <summary>
        /// 关闭MongoDB连接。
        /// </summary>
        public void Close()
        {
            Client.Cluster.Dispose();
        }
    }
}