using System.Collections.Generic;

namespace GameFrameX.GameAnalytics
{
    /// <summary>
    /// 
    /// </summary>
    public class GameAnalyticsManager : GameFrameworkModule, IGameAnalyticsManager
    {
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }

        public void Init()
        {
        }

        public void StartTimer(string eventName)
        {
        }

        public void StopTimer(string eventName)
        {
        }

        public void Event(string eventName)
        {
        }

        public void Event(string eventName, float eventValue)
        {
        }

        public void Event(string eventName, Dictionary<string, object> customF)
        {
        }

        public void Event(string eventName, float eventValue, Dictionary<string, object> customF)
        {
        }
    }
}