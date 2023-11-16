namespace Server.Core.Timer
{
    /// <summary>
    /// 跨天接口
    /// </summary>
    public interface ICrossDay
    {
        /// <summary>
        /// 在跨天时触发的方法
        /// </summary>
        /// <param name="openServerDay">开服天数</param>
        /// <returns>表示异步操作的任务</returns>
        public Task OnCrossDay(int openServerDay);
    }
}