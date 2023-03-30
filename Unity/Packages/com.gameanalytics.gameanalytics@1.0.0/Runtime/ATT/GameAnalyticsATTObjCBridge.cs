#if UNITY_IOS
using System.Runtime.InteropServices;

namespace GameAnalyticsSDK.iOS
{
    public static class GameAnalyticsATTObjCBridge
    {
        internal delegate void GameAnalyticsATTListenerNotDetermined();
        internal delegate void GameAnalyticsATTListenerRestricted();
        internal delegate void GameAnalyticsATTListenerDenied();
        internal delegate void GameAnalyticsATTListenerAuthorized();

        [DllImport("__Internal")]
        internal static extern void GameAnalyticsRequestTrackingAuthorization(
            GameAnalyticsATTListenerNotDetermined gameAnalyticsATTListenerNotDetermined,
            GameAnalyticsATTListenerRestricted gameAnalyticsATTListenerRestricted,
            GameAnalyticsATTListenerDenied gameAnalyticsATTListenerDenied,
            GameAnalyticsATTListenerAuthorized gameAnalyticsATTListenerAuthorized);
    }
}
#endif
