namespace Server.Utility
{
    /// <summary>
    /// 时间帮助工具类
    /// </summary>
    public static class TimeHelper
    {
        private static readonly DateTime epochLocal = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
        private static readonly DateTime epochUtc = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc);

        /// <summary>
        /// 返回当前时间的毫秒表示。
        /// </summary>
        public static long CurrentTimeMillis()
        {
            return TimeMillis(DateTime.Now, false);
        }

        /// <summary>
        /// 当前UTC 时间 秒时间戳
        /// </summary>
        /// <returns></returns>
        public static long UnixTimeSeconds()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        /// <summary>
        /// 当前UTC 时间 毫秒时间戳 
        /// </summary>
        /// <returns></returns>
        public static long UnixTimeMilliseconds()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 当前时区时间 秒时间戳
        /// </summary>
        /// <returns></returns>
        public static long TimeSeconds()
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        }

        /// <summary>
        /// 当前时区时间 毫秒时间戳 
        /// </summary>
        /// <returns></returns>
        public static long TimeMilliseconds()
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 获取当前时间距离纪元时间（UTC时间）的毫秒数。
        /// </summary>
        /// <returns>距离纪元时间的毫秒数。</returns>
        public static long CurrentTimeMillisWithUTC()
        {
            return TimeMillis(DateTime.UtcNow, true);
        }

        /// <summary>
        /// 获取当前时间距离纪元时间（UTC时间）的秒数。
        /// </summary>
        /// <returns>距离纪元时间的秒数。</returns>
        public static int CurrentTimeSecondWithUTC()
        {
            return TimeSecond(DateTime.UtcNow, true);
        }

        /// <summary>
        /// 获取指定时间距离纪元时间（本地时间或UTC时间）的毫秒数。
        /// </summary>
        /// <param name="time">指定时间。</param>
        /// <param name="utc">是否使用UTC时间。</param>
        /// <returns>距离纪元时间的毫秒数。</returns>
        public static long TimeMillis(DateTime time, bool utc = false)
        {
            if (utc)
                return (long)(time - epochUtc).TotalMilliseconds;
            return (long)(time - epochLocal).TotalMilliseconds;
        }

        /// <summary>
        /// 获取指定时间距离纪元时间（本地时间或UTC时间）的秒数。
        /// </summary>
        /// <param name="time">指定时间。</param>
        /// <param name="utc">是否使用UTC时间。</param>
        /// <returns>距离纪元时间的秒数。</returns>
        public static int TimeSecond(DateTime time, bool utc = false)
        {
            if (utc)
                return (int)(time - epochUtc).TotalSeconds;
            return (int)(time - epochLocal).TotalSeconds;
        }

        /// <summary>
        /// 毫秒转时间
        /// </summary>
        /// <param name="time"></param>
        /// <param name="utc"></param>
        /// <returns></returns>
        public static DateTime MillisToDateTime(long time, bool utc = false)
        {
            if (utc)
                return epochUtc.AddMilliseconds(time);
            return epochLocal.AddMilliseconds(time);
        }


        /// <summary>
        /// 获取从指定日期到当前日期之间跨越的天数。
        /// </summary>
        /// <param name="begin">起始日期。</param>
        /// <param name="hour">小时。</param>
        /// <returns>跨越的天数。</returns>
        public static int GetCrossDays(DateTime begin, int hour = 0)
        {
            return GetCrossDays(begin, DateTime.Now, hour);
        }

        /// <summary>
        /// 获取两个日期之间跨越的天数。
        /// </summary>
        /// <param name="begin">起始日期。</param>
        /// <param name="after">结束日期。</param>
        /// <param name="hour">小时。</param>
        /// <returns>跨越的天数。</returns>
        public static int GetCrossDays(DateTime begin, DateTime after, int hour = 0)
        {
            int days = (int)(after.Date - begin.Date).TotalDays;
            if (begin.Hour < hour)
                days++;
            if (after.Hour < hour)
                days--;
            return days;
        }

        /// <summary>
        /// 判断当前时间是否与指定时间处于同一周。
        /// </summary>
        /// <param name="start">指定时间的起始时间。</param>
        /// <returns>如果当前时间与指定时间处于同一周，则为 true；否则为 false。</returns>
        public static bool IsNowSameWeek(long start)
        {
            return IsNowSameWeek(new DateTime(start));
        }

        /// <summary>
        /// 判断当前时间是否与指定时间处于同一周。
        /// </summary>
        /// <param name="start">指定时间的起始时间。</param>
        /// <returns>如果当前时间与指定时间处于同一周，则为 true；否则为 false。</returns>
        public static bool IsNowSameWeek(DateTime start)
        {
            return IsSameWeek(start, DateTime.Now);
        }

        /// <summary>
        /// 是否同一周
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool IsSameWeek(DateTime start, DateTime end)
        {
            // 让start是较早的时间
            if (start > end)
            {
                (start, end) = (end, start);
            }

            int dayOfWeek = (int)start.DayOfWeek;
            if (dayOfWeek == (int)DayOfWeek.Sunday) dayOfWeek = 7;
            // 获取较早时间所在星期的星期天的0点
            var startsWeekLastDate = start.AddDays(7 - dayOfWeek).Date;
            // 判断end是否在start所在周
            return startsWeekLastDate >= end.Date;
        }

        /// <summary>
        /// 获取指定日期所在星期的时间。
        /// </summary>
        /// <param name="t">指定日期。</param>
        /// <param name="d">星期几。</param>
        /// <returns>指定日期所在星期的时间。</returns>
        public static DateTime GetDayOfWeekTime(DateTime t, DayOfWeek d)
        {
            int dd = (int)d;
            if (dd == 0) dd = 7;
            var dayOfWeek = (int)t.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7;
            return t.AddDays(dd - dayOfWeek).Date;
        }

        /// <summary>
        /// 获取当前日期所在星期的时间。
        /// </summary>
        /// <param name="d">星期几。</param>
        /// <returns>当前日期所在星期的时间。</returns>
        public static DateTime GetDayOfWeekTime(DayOfWeek d)
        {
            return GetDayOfWeekTime(DateTime.Now, d);
        }

        /// <summary>
        /// 获取指定星期在中国的对应数字。
        /// </summary>
        /// <param name="d">星期几。</param>
        /// <returns>星期在中国的对应数字。</returns>
        public static int GetChinaDayOfWeek(DayOfWeek d)
        {
            int dayOfWeek = (int)d;
            if (dayOfWeek == 0) dayOfWeek = 7;
            return dayOfWeek;
        }

        /// <summary>
        /// 获取当前星期在中国的对应数字。
        /// </summary>
        /// <returns>当前星期在中国的对应数字。</returns>
        public static int GetChinaDayOfWeek()
        {
            return GetChinaDayOfWeek(DateTime.Now.DayOfWeek);
        }

        /// <summary>
        /// 当前时区时间的完整字符串
        /// </summary>
        /// <returns></returns>
        public static string CurrentTimeWithFullString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss.fff K");
        }

        /// <summary>
        /// UTC时区时间的完整字符串
        /// </summary>
        /// <returns></returns>
        public static string CurrentTimeWithUtcFullString()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss.fff K");
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