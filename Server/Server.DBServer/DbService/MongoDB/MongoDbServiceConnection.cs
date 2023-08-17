using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NLog;
using Server.DBServer.State;
using Server.Extension;

namespace Server.DBServer.DbService.MongoDB
{
    public class MongoDbServiceConnection : IGameDbService
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();

        public MongoClient Client { get; private set; }

        public IMongoDatabase CurrentDatabase { get; private set; }

        public void Open(string url, string dbName)
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(url);
                Client = new MongoClient(settings);
                CurrentDatabase = Client.GetDatabase(dbName);
                Log.Info($"初始化MongoDB服务完成 Url:{url} DbName:{dbName}");
            }
            catch (Exception)
            {
                Log.Error($"初始化MongoDB服务失败 Url:{url} DbName:{dbName}");
                throw;
            }
        }

        public async Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, id);
            var col = CurrentDatabase.GetCollection<TState>();
            using var cursor = await col.FindAsync(filter);
            var state = await cursor.FirstOrDefaultAsync();
            bool isNew = state == null;
            if (state == null && defaultGetter != null)
            {
                state = defaultGetter();
            }

            if (state == null)
            {
                state = new TState {Id = id};
            }

            state.AfterLoadFromDB(isNew);
            return state;
        }

        private static Expression<Func<TState, bool>> GetDefaultFindExpression<TState>(Expression<Func<TState, bool>> filter) where TState : CacheState, new()
        {
            Expression<Func<TState, bool>> expression = m => m.IsDeleted == false;
            if (filter != null)
            {
                expression = expression.And(filter);
            }

            return expression;
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public async Task<List<TState>> FindListAsync<TState>(Expression<Func<TState, bool>> filter) where TState : CacheState, new()
        {
            var result = new List<TState>();
            var collection = CurrentDatabase.GetCollection<TState>();
            using var cursor = await collection.FindAsync<TState>(GetDefaultFindExpression(filter));
            while (await cursor.MoveNextAsync())
            {
                result.AddRange(cursor.Current);
            }

            return result;
        }

        /// <summary>
        /// 查询单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public async Task<TState> FindAsync<TState>(Expression<Func<TState, bool>> filter) where TState : CacheState, new()
        {
            var collection = CurrentDatabase.GetCollection<TState>();
            using var cursor = await collection.FindAsync<TState>(GetDefaultFindExpression(filter));
            var state = await cursor.FirstOrDefaultAsync();
            return state;
        }

        /// <summary>
        /// 查询数据长度
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public async Task<long> CountAsync<TState>(Expression<Func<TState, bool>> filter) where TState : CacheState, new()
        {
            var collection = CurrentDatabase.GetCollection<TState>();
            var newFilter = GetDefaultFindExpression(filter);
            var count = await collection.CountDocumentsAsync<TState>(newFilter);
            return count;
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        public async Task DeleteAsync<TState>(TState state) where TState : CacheState, new()
        {
            var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, state.Id);
            var collectionName = typeof(TState).Name;
            var collection = CurrentDatabase.GetCollection<TState>(collectionName);
            state.DeleteTime = DateTime.UtcNow;
            state.IsDeleted = true;
            _ = await collection.ReplaceOneAsync(filter, state, ReplaceOptions);
        }

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        public async Task AddAsync<TState>(TState state) where TState : CacheState, new()
        {
            var collectionName = typeof(TState).Name;
            var collection = CurrentDatabase.GetCollection<TState>(collectionName);
            state.CreateTime = DateTime.UtcNow;
            await collection.InsertOneAsync(state);
        }

        /// <summary>
        /// 增加一个列表数据
        /// </summary>
        /// <param name="states"></param>
        /// <typeparam name="TState"></typeparam>
        public async Task AddListAsync<TState>(List<TState> states) where TState : CacheState, new()
        {
            var collectionName = typeof(TState).Name;
            var collection = CurrentDatabase.GetCollection<TState>(collectionName);

            var cacheStates = states.ToList();
            foreach (var cacheState in cacheStates)
            {
                cacheState.CreateTime = DateTime.UtcNow;
            }

            await collection.InsertManyAsync(cacheStates);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public async Task SaveAsync<TState>(TState state) where TState : CacheState, new()
        {
            var (isChanged, data) = state.IsChanged();
            if (isChanged)
            {
                var cacheState = BsonSerializer.Deserialize<TState>(data);
                cacheState.UpdateTime = DateTime.UtcNow;
                cacheState.UpdateCount++;
                var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, state.Id);
                var col = CurrentDatabase.GetCollection<TState>();
                var result = await col.ReplaceOneAsync(filter, cacheState, ReplaceOptions);
                if (result.IsAcknowledged)
                {
                    state.AfterSaveToDB();
                }
            }
        }

        public static readonly ReplaceOptions ReplaceOptions = new() {IsUpsert = true};

        public static readonly BulkWriteOptions BulkWriteOptions = new() {IsOrdered = false};

        public Task CreateIndex<TState>(string indexKey)
        {
            var col = CurrentDatabase.GetCollection<TState>();
            var key = Builders<TState>.IndexKeys.Ascending(indexKey);
            var model = new CreateIndexModel<TState>(key);
            return col.Indexes.CreateOneAsync(model);
        }

        public void Close()
        {
            Client.Cluster.Dispose();
        }
    }
}