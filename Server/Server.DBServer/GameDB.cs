using System.Linq.Expressions;
using Server.DBServer.State;

namespace Server.DBServer
{
    public static class GameDb
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private static IGameDbService _dbServiceImpler;

        public static void Init(IGameDbService dbService)
        {
            _dbServiceImpler = dbService;
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

        public static Task<List<TState>> FindListAsync<TState>(Expression<Func<TState, bool>> filter) where TState : CacheState, new()
        {
            return _dbServiceImpler.FindListAsync<TState>(filter);
        }

        public static Task<long> CountAsync<TState>(Expression<Func<TState, bool>> filter) where TState : CacheState, new()
        {
            return _dbServiceImpler.CountAsync<TState>(filter);
        }

        public static Task<TState> FindAsync<TState>(Expression<Func<TState, bool>> filter) where TState : CacheState, new()
        {
            return _dbServiceImpler.FindAsync<TState>(filter);
        }

        public static Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : CacheState, new()
        {
            return _dbServiceImpler.LoadState(id, defaultGetter);
        }

        public static Task SaveState<TState>(TState state) where TState : CacheState, new()
        {
            return _dbServiceImpler.SaveAsync<TState>(state);
        }

        public static Task SaveOneAsync<TState>(TState state) where TState : CacheState, new()
        {
            return _dbServiceImpler.AddAsync<TState>(state);
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
}