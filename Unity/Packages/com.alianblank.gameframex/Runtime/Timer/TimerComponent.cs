using System;
using GameFrameX.Timer;
using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 计时器组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Timer")]
    public class TimerComponent : GameFrameworkComponent
    {
        ITimerManager _timerManager;

        protected override void Awake()
        {
            base.Awake();
            new TimerManager();
            _timerManager = GameFrameworkEntry.GetModule<ITimerManager>();
            if (_timerManager == null)
            {
                Log.Fatal("Timer manager is invalid.");
                return;
            }
        }

        /// <summary>
        /// 添加一个定时调用的任务
        /// </summary>
        /// <param name="interval">间隔时间（以毫秒为单位）</param>
        /// <param name="repeat">重复次数（0 表示无限重复）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        public void Add(float interval, int repeat, Action<object> callback, object callbackParam = null)
        {
            _timerManager.Add(interval, repeat, callback, callbackParam);
        }

        /// <summary>
        /// 添加一个只执行一次的任务
        /// </summary>
        /// <param name="interval">间隔时间（以毫秒为单位）</param>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数（可选）</param>
        public void AddOnce(float interval, Action<object> callback, object callbackParam = null)
        {
            _timerManager.AddOnce(interval, callback, callbackParam);
        }

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        public void AddUpdate(Action<object> callback)
        {
            _timerManager.AddUpdate(callback);
        }

        /// <summary>
        /// 添加一个每帧更新执行的任务
        /// </summary>
        /// <param name="callback">要执行的回调函数</param>
        /// <param name="callbackParam">回调函数的参数</param>
        public void AddUpdate(Action<object> callback, object callbackParam)
        {
            _timerManager.AddUpdate(callback, callbackParam);
        }

        /// <summary>
        /// 检查指定的任务是否存在
        /// </summary>
        /// <param name="callback">要检查的回调函数</param>
        /// <returns>存在返回 true，不存在返回 false</returns>
        public bool Exists(Action<object> callback)
        {
            return _timerManager.Exists(callback);
        }

        /// <summary>
        /// 移除指定的任务
        /// </summary>
        /// <param name="callback">要移除的回调函数</param>
        public void Remove(Action<object> callback)
        {
            _timerManager.Remove(callback);
        }
    }
}