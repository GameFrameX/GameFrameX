using Server.Core.Actors;
using Server.Setting;
using Server.Utility;

namespace Server.Core.Utility
{
    /// <summary>
    /// ActorId
    ///     14   +   7  + 30 +  12   = 63
    ///     服务器id 类型 时间戳 自增
    ///         玩家
    ///         公会
    ///     服务器id * 1000 + 全局功能id
    ///         全局玩法
    /// </summary>
    public static class IdGenerator
    {
        private static long genSecond = 0L;
        private static long incrNum = 0L;

        //此时间决定可用id年限(最晚有效年限=34年+此时间)(可调整,早于开服时间就行)
        private readonly static DateTime utcTimeStart = new(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly object lockObj = new();

        public static int GetServerId(long id)
        {
            return (int) (id < GlobalConst.MAX_GLOBAL_ID ? id / 1000 : id >> GlobalConst.SERVERID_OR_MODULEID_MASK);
        }

        public static DateTime GetGenerateTime(long id, bool utc = false)
        {
            if (id < GlobalConst.MAX_GLOBAL_ID)
            {
                throw new ArgumentException($"input is a global id:{id}");
            }

            var serverId = GetServerId(id);
            long seconds;
            if (serverId < GlobalConst.MIN_SERVER_ID)
            {
                // IDModule unique_id
                seconds = (id >> GlobalConst.MODULEID_TIMESTAMP_MASK) & GlobalConst.SECOND_MASK;
            }
            else
            {
                seconds = (id >> GlobalConst.TIMESTAMP_MASK) & GlobalConst.SECOND_MASK;
            }

            var date = utcTimeStart.AddSeconds(seconds);
            return utc ? date : date.ToLocalTime();
        }

        public static ActorType GetActorType(long id)
        {
            if (id <= 0)
            {
                throw new ArgumentException($"input id error:{id}");
            }

            if (id < GlobalConst.MAX_GLOBAL_ID)
            {
                // 全局actor
                return (ActorType) (id % 1000);
            }

            return (ActorType) ((id >> GlobalConst.ACTORTYPE_MASK) & 0xF);
        }

        public static long GetActorID(int type, int serverId = 0)
        {
            return GetActorID((ActorType) type, serverId);
        }

        public static long GetActorID(ActorType type, int serverId = 0)
        {
            if (type == ActorType.Separator)
            {
                throw new ArgumentException($"input actor type error: {type}");
            }

            if (serverId < 0)
            {
                throw new ArgumentException($"serverId negtive when generate id {serverId}");
            }
            else if (serverId == 0)
            {
                serverId = GlobalSettings.ServerId;
            }

            if (type < ActorType.Separator)
            {
                return GetMultiActorID(type, serverId);
            }
            else
            {
                return GetGlobalActorID(type, serverId);
            }
        }

        public static long GetMultiActorIDBegin(ActorType type)
        {
            if (type >= ActorType.Separator)
            {
                throw new ArgumentException($"input actor type error: {type}");
            }

            var id = (long) GlobalSettings.ServerId << GlobalConst.SERVERID_OR_MODULEID_MASK;
            id |= (long) type << GlobalConst.ACTORTYPE_MASK;
            return id;
        }

        private static long GetGlobalActorID(ActorType type, int serverId)
        {
            return (long) (serverId * 1000 + type);
        }


        private static long GetMultiActorID(ActorType type, int serverId)
        {
            long second = (long) (DateTime.UtcNow - utcTimeStart).TotalSeconds;
            lock (lockObj)
            {
                if (second > genSecond)
                {
                    genSecond = second;
                    incrNum = 0L;
                }
                else if (incrNum >= GlobalConst.MAX_ACTOR_INCREASE)
                {
                    ++genSecond;
                    incrNum = 0L;
                }
                else
                {
                    ++incrNum;
                }
            }

            var id = (long) serverId << GlobalConst.SERVERID_OR_MODULEID_MASK; // serverId-14位, 支持1000~9999
            id |= (long) type << GlobalConst.ACTORTYPE_MASK; // 多actor类型-7位, 支持0~127
            id |= genSecond << GlobalConst.TIMESTAMP_MASK; // 时间戳-30位, 支持34年
            id |= incrNum; // 自增-12位, 每秒4096个
            return id;
        }

        public static long GetUniqueId(IDModule module)
        {
            long second = (long) (DateTime.UtcNow - utcTimeStart).TotalSeconds;
            lock (lockObj)
            {
                if (second > genSecond)
                {
                    genSecond = second;
                    incrNum = 0L;
                }
                else if (incrNum >= GlobalConst.MAX_UNIQUE_INCREASE)
                {
                    ++genSecond;
                    incrNum = 0L;
                }
                else
                {
                    ++incrNum;
                }
            }

            var id = (long) module << GlobalConst.SERVERID_OR_MODULEID_MASK; // 模块id 14位 支持 0~9999
            id |= genSecond << GlobalConst.MODULEID_TIMESTAMP_MASK; // 时间戳 30位
            id |= incrNum; // 自增 19位
            return id;
        }
    }
}