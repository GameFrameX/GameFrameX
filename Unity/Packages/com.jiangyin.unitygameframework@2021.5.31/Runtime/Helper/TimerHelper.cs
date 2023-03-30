using System;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 游戏时间帮助类
    /// </summary>
    public static class GameTimeHelper
    {
        private static readonly long epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        /// <summary>
        /// 时间差
        /// </summary>
        private static long _differenceTime;

        private static bool _isSecLevel = true;

        // public const long OneSecond = 1000;
        // public const long OneSecond = 1000;
        public const long TicksPerMicrosecond = 1; //100微秒
        public const long TicksPer = 10 * TicksPerMicrosecond;
        public const long TicksPerMillisecond = TicksPer * 1000; //毫秒
        public const long TicksPerSecond = TicksPerMillisecond * 1000; // 秒  //10000000

        /// <summary>
        /// 设置时间差
        /// </summary>
        /// <param name="timeSpan"></param>
        public static void SetDifferenceTime(long timeSpan)
        {
            if (timeSpan > 1000000000000)
            {
                _isSecLevel = false;
            }
            else
            {
                _isSecLevel = true;
            }

            if (_isSecLevel)
            {
                _differenceTime = timeSpan - ClientNow();
            }
            else
            {
                _differenceTime = timeSpan - ClientNowMillisecond();
            }
        }

        /// <summary>
        /// 毫秒级
        /// </summary>
        /// <returns></returns>
        public static long ClientNowMillisecond()
        {
            return (DateTime.UtcNow.Ticks - epoch) / TicksPerMillisecond;
        }

        public static long ServerToday()
        {
            if (_isSecLevel)
            {
                return _differenceTime + ClientToday();
            }

            return (_differenceTime + ClientTodayMillisecond()) / 1000;
        }

        public static long ClientTodayMillisecond()
        {
            return (DateTime.Now.Date.ToUniversalTime().Ticks - epoch) / 10000;
        }

        /// <summary>
        /// 服务器当前时间
        /// </summary>
        /// <returns></returns>
        public static long ServerNow() //秒级
        {
            if (_isSecLevel)
            {
                return _differenceTime + ClientNow();
            }

            return (_differenceTime + ClientNowMillisecond()) / 1000;
        }

        public static long ClientToday()
        {
            return (DateTime.Now.Date.ToUniversalTime().Ticks - epoch) / TicksPerSecond;
        }

        /// <summary>
        /// 客户端时间
        /// </summary>
        /// <returns></returns>
        public static long ClientNow()
        {
            return (DateTime.UtcNow.Ticks - epoch) / 10000;
        }

        public static long ClientNowSeconds()
        {
            return (DateTime.UtcNow.Ticks - epoch) / 10000000;
        }

        public static long Now()
        {
            return ClientNow();
        }
    }
}