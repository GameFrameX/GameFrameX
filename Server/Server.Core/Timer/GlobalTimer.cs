using Server.Core.Actors;
using Server.Core.Utility;
using Server.DBServer;
using Server.Extension;
using Server.Setting;
using Server.Utility;

namespace Server.Core.Timer
{
    public static class GlobalTimer
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static Task LoopTask;
        public static volatile bool working = false;

        public static void Start()
        {
            working = true;
            LoopTask = Task.Run(Loop);
            Log.Info($"初始化全局定时完成");
        }

        private static async Task Loop()
        {
            var nextSaveTime = NextSaveTime();
            var saveInterval = TimeSpan.FromMilliseconds(GlobalConst.SAVE_INTERVAL_IN_MilliSECONDS);
            var ONCE_DELAY = TimeSpan.FromMilliseconds(200);
            while (working)
            {
                Log.Info($"下次定时回存时间 {nextSaveTime}");
                while (DateTime.Now < nextSaveTime && working)
                {
                    await Task.Delay(ONCE_DELAY);
                }

                if (!working)
                    break;
                var startTime = DateTime.Now;
                await GameDb.TimerSave();
                var cost = (DateTime.Now - startTime).TotalMilliseconds;
                Log.Info($"定时回存完成 耗时: {cost:f4}ms");

                await ActorMgr.CheckIdle();

                do
                {
                    nextSaveTime = nextSaveTime.Add(saveInterval);
                } while (DateTime.Now > nextSaveTime);
            }
        }

        private static DateTime NextSaveTime()
        {
            var now = DateTime.Now;
            var t = now.Date.AddHours(now.Hour);
            while (t < now)
            {
                t = t.AddMilliseconds(GlobalConst.SAVE_INTERVAL_IN_MilliSECONDS);
            }

            int serverId = GlobalSettings.ServerId;
            int a = serverId % 1000;
            int b = a % GlobalConst.MAGIC;
            int c = GlobalConst.SAVE_INTERVAL_IN_MilliSECONDS / GlobalConst.MAGIC;
            int r = ThreadLocalRandom.Current.Next(0, c);
            int delay = b * c + r;
            t = t.AddMilliseconds(delay);
            if ((t - now).TotalMilliseconds > GlobalConst.SAVE_INTERVAL_IN_MilliSECONDS)
            {
                t = t.AddMilliseconds(-GlobalConst.SAVE_INTERVAL_IN_MilliSECONDS);
            }

            return t;
        }


        public static async Task Stop()
        {
            working = false;
            await LoopTask;
            await GameDb.SaveAll();
            GameDb.Close();
            Log.Info($"停止全局定时完成");
        }
    }
}