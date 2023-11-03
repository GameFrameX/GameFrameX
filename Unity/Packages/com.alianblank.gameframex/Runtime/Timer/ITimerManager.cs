using System;

namespace GameFrameX.Timer
{
    /// <summary>
    /// 定时器接口
    /// </summary>
    public interface ITimerManager
    {
        /// <summary>
        /// 添加一个定时调用的任务
        /// </summary>
        /// <param name="interval">间隔时间（以毫秒为单位）</param>
        /// <param name="repeat">重复次数（0 表示无限重复）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        void Add(float interval, int repeat, Action<object> callback, object callbackParam = null);

        /// <summary>
        /// 添加一个只执行一次的任务
        /// </summary>
        /// <param name="interval">间隔时间（以毫秒为单位）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        void AddOnce(float interval, Action<object> callback, object callbackParam = null);

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        void AddUpdate(Action<object> callback);

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数</param>
        void AddUpdate(Action<object> callback, object callbackParam);

        /// <summary>
        /// 检查指定的任务是否存在
        /// </summary>
        /// <param name="callback">要检查的回调函数</param>
        /// <returns>存在返回 true，不存在返回 false</returns>
        bool Exists(Action<object> callback);

        /// <summary>
        /// 移除指定的任务
        /// </summary>
        /// <param name="callback">要移除的回调函数</param>
        void Remove(Action<object> callback);
    }
}