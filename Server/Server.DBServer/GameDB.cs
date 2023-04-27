namespace Geek.Server.Core.Storage
{
    public enum DBModel
    {
        /// <summary>
        /// 内嵌做主存,mongodb备份
        /// </summary>
        Embeded,

        /// <summary>
        /// mongodb主存,存储失败再存内嵌
        /// </summary>
        Mongodb,
    }

    public interface IGameDB
    {
        public void Open(string url, string dbName);
        public void Close();
        public Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new();
        public Task SaveState<TState>(TState state) where TState : CacheState;
    }

    public class GameDB
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private static IGameDB dbImpler;


        public static void Init()
        {
            dbImpler = new MongoDBConnection();
        }

        public static T As<T>() where T : IGameDB
        {
            return (T) dbImpler;
        }

        public static void Open(string MongoUrl,string MongoDBName)
        {
            dbImpler.Open(MongoUrl, MongoDBName);
        }

        public static void Close()
        {
            dbImpler.Close();
        }

        public static Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            return dbImpler.LoadState(id, defaultGetter);
        }

        public static Task SaveState<TState>(TState state) where TState : CacheState
        {
            return dbImpler.SaveState(state);
        }


        public static async Task SaveAll()
        {
            // await StateComp.SaveAll();
        }

        public static async Task TimerSave()
        {
            // await StateComp.TimerSave();
        }
    }
}