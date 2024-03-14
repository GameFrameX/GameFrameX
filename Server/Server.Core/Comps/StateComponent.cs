using System.Collections.Concurrent;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;

using Server.Core.Actors;
using Server.Core.Timer;
using Server.Core.Utility;
using Server.DBServer;
using Server.DBServer.DbService.MongoDB;
using Server.DBServer.State;
using Server.DBServer.Storage;
using Server.Extension;
using Server.Log;
using Server.Setting;
using Server.Utility;

namespace Server.Core.Comps
{
    public sealed class StateComponent
    {
        #region 仅DBModel.Mongodb调用


        private static readonly ConcurrentBag<Func<bool, bool, Task>> saveFuncs = new();

        public static void AddShutdownSaveFunc(Func<bool, bool, Task> shutdown)
        {
            saveFuncs.Add(shutdown);
        }

        /// <summary>
        /// 当游戏出现异常，导致无法正常回存，才需要将force=true
        /// 由后台http指令调度
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        public static async Task SaveAll(bool force = false)
        {
            try
            {
                var begin = DateTime.Now;
                var tasks = new List<Task>();
                foreach (var saveFunc in saveFuncs)
                {
                    tasks.Add(saveFunc(true, force));
                }

                await Task.WhenAll(tasks);
                LogHelper.Info($"save all state, use: {(DateTime.Now - begin).TotalMilliseconds}ms");
            }
            catch (Exception e)
            {
                LogHelper.Error($"save all state error \n{e}");
            }
        }

        /// <summary>
        /// 定时回存所有数据
        /// </summary>
        public static async Task TimerSave()
        {
            try
            {
                foreach (var func in saveFuncs)
                {
                    await func(false, false);
                    if (!GlobalTimer.working)
                        return;
                }
            }
            catch (Exception e)
            {
                LogHelper.Info("timer save state error");
                LogHelper.Error(e.ToString());
            }
        }

        public static readonly StatisticsTool statisticsTool = new();

        #endregion
    }

    public abstract class StateComponent<TState> : BaseComponent, IState where TState : CacheState, new()
    {
        static readonly ConcurrentDictionary<long, TState> stateDic = new();

        public TState State { get; private set; }

        static StateComponent()
        {
            // StateComponent.AddShutdownSaveFunc(SaveAll);
        }

        public override async Task Active()
        {
            await base.Active();
            if (State != null)
            {
                return;
            }

            await ReadStateAsync();
        }

        public override Task Inactive()
        {
            // if (GlobalSettings.DBModel == (int) DbModel.Mongodb)
            stateDic.TryRemove(ActorId, out _);
            return base.Inactive();
        }


        internal override bool ReadyToInactive => State == null || !State.IsChanged().isChanged;

        internal override async Task SaveState()
        {
            try
            {
                // await GameDb.UpdateAsync(State);
            }
            catch (Exception e)
            {
                LogHelper.Fatal($"StateComp.SaveState.Failed.StateId:{State.Id},{e}");
            }
        }

        public async Task ReadStateAsync()
        {
            /*State = await GameDb.LoadState<TState>(ActorId);
            // if (GlobalSettings.DBModel == (int)DbModel.Mongodb)
            {
                stateDic.TryRemove(State.Id, out _);
                stateDic.TryAdd(State.Id, State);
            }*/
        }

        /*public Task WriteStateAsync()
        {
            return GameDb.UpdateAsync(State);
        }*/


        #region 仅DBModel.Mongodb调用

        /*
        const int ONCE_SAVE_COUNT = 500;

        public static async Task SaveAll(bool shutdown, bool force = false)
        {
            static void AddReplaceModel(List<ReplaceOneModel<MongoState>> writeList, bool isChanged, long stateId, byte[] data)
            {
                if (isChanged)
                {
                    var mongoState = new MongoState()
                    {
                        Data = data,
                        Id = stateId.ToString(),
                        Timestamp = TimeHelper.CurrentTimeMillisWithUTC()
                    };
                    var filter = Builders<MongoState>.Filter.Eq(CacheState.UniqueId, mongoState.Id);
                    writeList.Add(new ReplaceOneModel<MongoState>(filter, mongoState) { IsUpsert = true });
                }
            }

            var writeList = new List<ReplaceOneModel<MongoState>>();
            var tasks = new List<Task<(bool, long, byte[])>>();

            foreach (var state in stateDic.Values)
            {
                var actor = ActorManager.GetActor(state.Id);
                if (actor != null)
                {
                    if (force)
                    {
                        var (isChanged, data) = state.IsChanged();
                        AddReplaceModel(writeList, isChanged, state.Id, data);
                    }
                    else
                    {
                        tasks.Add(actor.SendAsync(() => state.IsChangedWithId()));
                    }
                }
            }

            var results = await Task.WhenAll(tasks);
            foreach (var (isChanged, stateId, data) in results)
            {
                AddReplaceModel(writeList, isChanged, stateId, data);
            }

            if (!writeList.IsNullOrEmpty())
            {
                var stateName = typeof(TState).FullName;
                StateComponent.statisticsTool.Count(stateName, writeList.Count);
                LogHelper.Debug($"状态回存 {stateName} count:{writeList.Count}");
                var currentDatabase = GameDb.As<MongoDbServiceConnection>().CurrentDatabase;
                var mongoCollection = currentDatabase.GetCollection<MongoState>(stateName);
                for (int idx = 0; idx < writeList.Count; idx += ONCE_SAVE_COUNT)
                {
                    var list = writeList.GetRange(idx, Math.Min(ONCE_SAVE_COUNT, writeList.Count - idx));

                    bool save = false;
                    try
                    {
                        var result = await mongoCollection.BulkWriteAsync(list, MongoDbServiceConnection.BulkWriteOptions);
                        if (result.IsAcknowledged)
                        {
                            list.ForEach(model =>
                            {
                                if (stateDic.TryGetValue(long.Parse(model.Replacement.Id), out var cacheState))
                                {
                                    cacheState.AfterSaveToDB();
                                }
                            });
                            save = true;
                        }
                        else
                        {
                            LogHelper.Error($"保存数据失败，类型:{typeof(TState).FullName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"保存数据异常，类型:{typeof(TState).FullName}，{ex}");
                    }

                    if (!save && shutdown)
                    {
                        await FileBackup.SaveToFile(list, stateName);
                    }
                }
            }
        }*/


        // public static async Task<MongoState> FindAsync<TState>(FilterDefinition<BsonDocument> filter, int limit = 0)
        // {
        //     var stateName = typeof(TState).FullName;
        //     var _database = GameDb.As<MongoDbServiceConnection>().CurrentDatabase;
        //     var collection = _database.GetCollection<MongoState>(stateName);
        //     // var result = new List<BsonDocument>();
        //     using (var cursor = await collection.FindAsync(filter, new FindOptions<MongoState> {Limit = limit}))
        //     {
        //         return await cursor.FirstOrDefaultAsync();
        //     }
        // }

        #endregion
    }
}