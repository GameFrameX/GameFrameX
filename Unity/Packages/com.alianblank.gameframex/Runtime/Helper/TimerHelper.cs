using System;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 游戏时间帮助类
    /// </summary>
    public static class GameTimeHelper
    {
        private static readonly long Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

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
            return (DateTime.UtcNow.Ticks - Epoch) / TicksPerMillisecond;
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
            return (DateTime.Now.Date.ToUniversalTime().Ticks - Epoch) / 10000;
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

        /// <summary>
        /// 今天的客户端时间
        /// </summary>
        /// <returns></returns>
        public static long ClientToday()
        {
            return (DateTime.Now.Date.ToUniversalTime().Ticks - Epoch) / TicksPerSecond;
        }

        /// <summary>
        /// 客户端时间，毫秒
        /// </summary>
        /// <returns></returns>
        public static long ClientNow()
        {
            return (DateTime.UtcNow.Ticks - Epoch) / 10000;
        }

        /// <summary>
        /// 客户端时间。秒
        /// </summary>
        /// <returns></returns>
        public static long ClientNowSeconds()
        {
            return (DateTime.UtcNow.Ticks - Epoch) / 10000000;
        }

        /// <summary>
        /// 客户端时间
        /// </summary>
        /// <returns></returns>
        public static long Now()
        {
            return ClientNow();
        }

        /// <summary>
        /// UTC时间 秒
        /// </summary>
        /// <returns></returns>
        public static long UnixTimeSeconds()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        /// <summary>
        /// UTC时间 毫秒
        /// </summary>
        /// <returns></returns>
        public static long UnixTimeMilliseconds()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }


        /// <summary>
        /// 按照UTC时间判断两个时间戳是否是同一天
        /// </summary>
        /// <param name="timestamp1">时间戳1</param>
        /// <param name="timestamp2">时间戳2</param>
        /// <returns>是否是同一天</returns>
        public static bool IsUnixSameDay(long timestamp1, long timestamp2)
        {
            var time1 = UtcToUtcDateTime(timestamp1);
            var time2 = UtcToUtcDateTime(timestamp2);
            return time1.Date.Year == time2.Date.Year && time1.Date.Month == time2.Date.Month && time1.Date.Day == time2.Date.Day;
        }

        /// <summary>
        /// 按照本地时间判断两个时间戳是否是同一天
        /// </summary>
        /// <param name="timestamp1">时间戳1</param>
        /// <param name="timestamp2">时间戳2</param>
        /// <returns>是否是同一天</returns>
        public static bool IsLocalSameDay(long timestamp1, long timestamp2)
        {
            var time1 = UtcToLocalDateTime(timestamp1);
            var time2 = UtcToLocalDateTime(timestamp2);
            return time1.Date.Year == time2.Date.Year && time1.Date.Month == time2.Date.Month && time1.Date.Day == time2.Date.Day;
        }

        /// <summary>
        /// UTC 时间戳 转换成UTC时间
        /// </summary>
        /// <param name="utcTimestamp">UTC时间戳,单位秒</param>
        /// <returns></returns>
        public static DateTime UtcToUtcDateTime(long utcTimestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(utcTimestamp).UtcDateTime;
        }

        /// <summary>
        /// UTC 时间戳 转换成本地时间
        /// </summary>
        /// <param name="utcTimestamp">UTC时间戳,单位秒</param>
        /// <returns></returns>
        public static DateTime UtcToLocalDateTime(long utcTimestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(utcTimestamp).LocalDateTime;
        }

        /// <summary>
        /// 判断两个时间是否是同一天
        /// </summary>
        /// <param name="time1">时间1</param>
        /// <param name="time2">时间2</param>
        /// <returns>是否是同一天</returns>
        public static bool IsSameDay(DateTime time1, DateTime time2)
        {
            return time1.Date.Year == time2.Date.Year && time1.Date.Month == time2.Date.Month && time1.Date.Day == time2.Date.Day;
        }
    }
}