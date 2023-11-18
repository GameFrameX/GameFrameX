using System.Collections.Generic;
using GameFrameX.GameAnalytics;
using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 游戏数据分析组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/GameAnalytics")]
    public sealed class GameAnalyticsComponent : GameFrameworkComponent
    {
        private bool _isInit = false;
        private IGameAnalyticsManager _gameAnalyticsManager;

        protected override void Awake()
        {
            base.Awake();
            _gameAnalyticsManager = GameFrameworkEntry.GetModule<IGameAnalyticsManager>();
            if (_gameAnalyticsManager == null)
            {
                Log.Fatal("GameAnalytics manager is invalid.");
                return;
            }

            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            _gameAnalyticsManager.Init();
            _isInit = true;
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void StartTimer(string eventName)
        {
            if (!_isInit)
            {
                return;
            }

            _gameAnalyticsManager.StartTimer(eventName);
        }

        /// <summary>
        /// 结束计时
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void StopTimer(string eventName)
        {
            if (!_isInit)
            {
                return;
            }

            _gameAnalyticsManager.StopTimer(eventName);
        }

        /// <summary>
        /// 上报事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void Event(string eventName)
        {
            if (!_isInit)
            {
                return;
            }

            _gameAnalyticsManager.Event(eventName);
        }

        /// <summary>
        /// 上报带有数值的事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="eventValue">事件数值</param>
        public void Event(string eventName, float eventValue)
        {
            if (!_isInit)
            {
                return;
            }

            _gameAnalyticsManager.Event(eventName, eventValue);
        }

        /// <summary>
        /// 上报自定义字段的事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="customF">自定义字段</param>
        public void Event(string eventName, Dictionary<string, string> customF)
        {
            if (!_isInit)
            {
                return;
            }

            var value = new Dictionary<string, object>();

            foreach (var kv in customF)
            {
                value[kv.Key] = kv.Value;
            }

            _gameAnalyticsManager.Event(eventName, value);
        }

        /// <summary>
        /// 上报带有数值和自定义字段的事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="eventValue">事件数值</param>
        /// <param name="customF">自定义字段</param>
        public void Event(string eventName, float eventValue, Dictionary<string, string> customF)
        {
            if (!_isInit)
            {
                return;
            }

            var value = new Dictionary<string, object>();

            foreach (var kv in customF)
            {
                value[kv.Key] = kv.Value;
            }

            _gameAnalyticsManager.Event(eventName, eventValue, value);
        }
    }
}