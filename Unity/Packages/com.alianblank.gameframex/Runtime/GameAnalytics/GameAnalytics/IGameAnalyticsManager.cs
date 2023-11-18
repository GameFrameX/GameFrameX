using System.Collections.Generic;

namespace GameFrameX.GameAnalytics
{
    /// <summary>
    /// 游戏数据分析组件。
    /// </summary>
    public interface IGameAnalyticsManager
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init();

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="eventName">事件名称</param>
        void StartTimer(string eventName);

        /// <summary>
        /// 结束计时
        /// </summary>
        /// <param name="eventName">事件名称</param>
        void StopTimer(string eventName);

        /// <summary>
        /// 上报事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        void Event(string eventName);

        /// <summary>
        /// 上报带有数值的事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="eventValue">事件数值</param>
        void Event(string eventName, float eventValue);

        /// <summary>
        /// 上报自定义字段的事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="customF">自定义字段</param>
        void Event(string eventName, Dictionary<string, object> customF);

        /// <summary>
        /// 上报带有数值和自定义字段的事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="eventValue">事件数值</param>
        /// <param name="customF">自定义字段</param>
        void Event(string eventName, float eventValue, Dictionary<string, object> customF);
    }
}