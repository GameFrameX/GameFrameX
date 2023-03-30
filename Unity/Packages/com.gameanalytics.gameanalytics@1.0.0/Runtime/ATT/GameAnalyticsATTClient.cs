#if UNITY_IOS
using AOT;

namespace GameAnalyticsSDK.iOS
{
    public class GameAnalyticsATTClient
    {
        #region Singleton

        private GameAnalyticsATTClient()
        {
        }

        public static GameAnalyticsATTClient Instance { get; } = new GameAnalyticsATTClient();

        #endregion

        private static IGameAnalyticsATTListener _attListener;

        [MonoPInvokeCallback(typeof(GameAnalyticsATTObjCBridge.GameAnalyticsATTListenerNotDetermined))]
        private static void GameAnalyticsATTListenerNotDetermined()
        {
            _attListener?.GameAnalyticsATTListenerNotDetermined();
        }

        [MonoPInvokeCallback(typeof(GameAnalyticsATTObjCBridge.GameAnalyticsATTListenerRestricted))]
        private static void GameAnalyticsATTListenerRestricted()
        {
            _attListener?.GameAnalyticsATTListenerRestricted();
        }

        [MonoPInvokeCallback(typeof(GameAnalyticsATTObjCBridge.GameAnalyticsATTListenerDenied))]
        private static void GameAnalyticsATTListenerDenied()
        {
            _attListener?.GameAnalyticsATTListenerDenied();
        }

        [MonoPInvokeCallback(typeof(GameAnalyticsATTObjCBridge.GameAnalyticsATTListenerAuthorized))]
        private static void GameAnalyticsATTListenerAuthorized()
        {
            _attListener?.GameAnalyticsATTListenerAuthorized();
        }

        public void RequestTrackingAuthorization(IGameAnalyticsATTListener listener)
        {
            _attListener = listener;
            GameAnalyticsATTObjCBridge.GameAnalyticsRequestTrackingAuthorization(GameAnalyticsATTListenerNotDetermined, GameAnalyticsATTListenerRestricted,
                GameAnalyticsATTListenerDenied, GameAnalyticsATTListenerAuthorized);
        }
    }
}
#endif
