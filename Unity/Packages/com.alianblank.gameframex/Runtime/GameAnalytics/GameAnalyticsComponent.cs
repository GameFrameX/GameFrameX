using System.Collections.Generic;
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
        private static bool _isInit = false;

        public static void Init()
        {
            _isInit = true;
        }

        public static void StartTimer(string eventName)
        {
            if (!_isInit)
            {
                return;
            }

            // GameAnalytics.NewDesignEvent(eventName);
        }

        public static void StopTimer(string eventName)
        {
            if (!_isInit)
            {
                return;
            }

            // GameAnalytics.NewDesignEvent(eventName);
        }

        public static void DesignEvent(string eventName)
        {
            if (!_isInit)
            {
                return;
            }

            // GameAnalytics.NewDesignEvent(eventName);
        }

        public static void DesignEvent(string eventName, float eventValue)
        {
            if (!_isInit)
            {
                return;
            }

            // GameAnalytics.NewDesignEvent(eventName, eventValue);
        }

        public static void DesignEvent(string eventName, Dictionary<string, string> customF)
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

            // GameAnalytics.NewDesignEvent(eventName, value);
        }

        public static void DesignEvent(string eventName, float eventValue, Dictionary<string, string> customF)
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

            // GameAnalytics.NewDesignEvent(eventName, eventValue, value);
        }
    }
}