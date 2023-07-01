using Server.DBServer.Storage;

namespace Server.DBServer
{
    public static class GameDb
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private static IGameDbService _dbServiceImpler;

        public static void Init()
        {
            _dbServiceImpler = new MongoDbServiceConnection();
        }

        public static T As<T>() where T : IGameDbService
        {
            return (T) _dbServiceImpler;
        }

        public static void Open(string mongoUrl, string mongoDbName)
        {
            _dbServiceImpler.Open(mongoUrl, mongoDbName);
        }

        public static void Close()
        {
            _dbServiceImpler.Close();
        }

        public static Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            return _dbServiceImpler.LoadState(id, defaultGetter);
        }

        public static Task SaveState<TState>(TState state) where TState : CacheState
        {
            return _dbServiceImpler.SaveState(state);
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