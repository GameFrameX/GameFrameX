using MongoDB.Bson.Serialization.Attributes;
using Server.DBServer.Storage;

namespace Server.DBServer.State
{
    [BsonSerializer]
    public class CacheState : ICacheState
    {
        public CacheState()
        {
        }

        public const string UniqueId = nameof(Id);

        /// <summary>
        /// 唯一ID
        /// </summary>
        [BsonId]
        public long Id { get; set; }

        public bool IsModify => IsChanged().isChanged;


        public override string ToString()
        {
            return $"{base.ToString()}[Id={Id}]";
        }

        #region hash

        private StateHash stateHash;

        public void AfterLoadFromDB(bool isNew)
        {
            stateHash = new StateHash(this, isNew);
        }

        public (bool isChanged, byte[] data) IsChanged()
        {
            return stateHash.IsChanged();
        }

        public (bool isChanged, long stateId, byte[] data) IsChangedWithId()
        {
            var res = stateHash.IsChanged();
            return (res.Item1, Id, res.Item2);
        }

        /// <summary>
        /// 仅DBModel.Mongodb时调用
        /// </summary>
        public void BeforeSaveToDB()
        {
            // var db = GameDb.As<RocksDBConnection>().CurDataBase;
            // var table = db.GetTable<SaveTimestamp>();
            // var saveState = new SaveTimestamp
            // {
            //     //此处使用UTC时间
            //     Timestamp = TimeUtils.CurrentTimeMillisUTC(),
            //     StateName = GetType().FullName,
            //     StateId = Id.ToString(),
            // };
            // table.Set(saveState.Key, saveState);
        }

        public void AfterSaveToDB()
        {
            stateHash.AfterSaveToDb();
        }

        #endregion

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public long DeleteTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreateId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreateTime { get; set; }

        /// <summary>
        /// 更新次数
        /// </summary>
        public int UpdateCount { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public long UpdateTime { get; set; }
    }
}