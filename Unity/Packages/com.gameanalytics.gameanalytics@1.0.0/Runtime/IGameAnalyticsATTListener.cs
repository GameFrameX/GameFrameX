

namespace GameAnalyticsSDK
{
    public interface IGameAnalyticsATTListener
    {
        void GameAnalyticsATTListenerNotDetermined();
        void GameAnalyticsATTListenerRestricted();
        void GameAnalyticsATTListenerDenied();
        void GameAnalyticsATTListenerAuthorized();
    }
}
