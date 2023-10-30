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
        /// <param name="eventName"></param>
        void StartTimer(string eventName);

        /// <summary>
        /// 结束计时
        /// </summary>
        /// <param name="eventName"></param>
        void StopTimer(string eventName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventName"></param>
        void DesignEvent(string eventName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventValue"></param>
        void DesignEvent(string eventName, float eventValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="customF"></param>
        void DesignEvent(string eventName, Dictionary<string, string> customF);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventValue"></param>
        /// <param name="customF"></param>
        void DesignEvent(string eventName, float eventValue, Dictionary<string, string> customF);
    }
}