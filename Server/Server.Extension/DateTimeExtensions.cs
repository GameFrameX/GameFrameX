namespace Server.Extension
{
    /// <summary>
    /// 提供对 <see cref="DateTime"/> 类型的扩展方法。
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 获取从指定日期到当前日期的天数差。
        /// </summary>
        /// <param name="now">当前日期。</param>
        /// <param name="dt">指定的日期。</param>
        /// <returns>从指定日期到当前日期的天数差。</returns>
        public static int GetDaysFrom(this DateTime now, DateTime dt)
        {
            return (int)(now.Date - dt).TotalDays;
        }

        /// <summary>
        /// 获取从默认日期（1970年1月1日）到当前日期的天数差。
        /// </summary>
        /// <param name="now">当前日期。</param>
        /// <returns>从默认日期到当前日期的天数差。</returns>
        public static int GetDaysFromDefault(this DateTime now)
        {
            return now.GetDaysFrom(new DateTime(1970, 1, 1).Date);
        }
    }
}