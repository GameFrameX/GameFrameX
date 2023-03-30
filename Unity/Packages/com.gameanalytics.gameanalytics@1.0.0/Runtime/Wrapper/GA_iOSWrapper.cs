using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace GameAnalyticsSDK.Wrapper
{
    public partial class GA_Wrapper
    {
#if (UNITY_IOS) && (!UNITY_EDITOR)

        [DllImport ("__Internal")]
        private static extern void configureAvailableCustomDimensions01(string list);

        [DllImport ("__Internal")]
        private static extern void configureAvailableCustomDimensions02(string list);

        [DllImport ("__Internal")]
        private static extern void configureAvailableCustomDimensions03(string list);

        [DllImport ("__Internal")]
        private static extern void configureAvailableResourceCurrencies(string list);

        [DllImport ("__Internal")]
        private static extern void configureAvailableResourceItemTypes(string list);

        [DllImport ("__Internal")]
        private static extern void configureSdkGameEngineVersion(string unitySdkVersion);

        [DllImport ("__Internal")]
        private static extern void configureGameEngineVersion(string unityEngineVersion);

        [DllImport ("__Internal")]
        private static extern void configureBuild(string build);

        [DllImport ("__Internal")]
        private static extern void configureUserId(string userId);

        [DllImport ("__Internal")]
        private static extern void configureAutoDetectAppVersion(bool flag);

        [DllImport ("__Internal")]
        private static extern void gaInitialize(string gamekey, string gamesecret, bool nativeErrorReporting);

        [DllImport ("__Internal")]
        private static extern void setCustomDimension01(string customDimension);

        [DllImport ("__Internal")]
        private static extern void setCustomDimension02(string customDimension);

        [DllImport ("__Internal")]
        private static extern void setCustomDimension03(string customDimension);

        [DllImport ("__Internal")]
        private static extern void setGlobalCustomEventFields(string customFields);

        [DllImport ("__Internal")]
        private static extern void addBusinessEvent(string currency, int amount, string itemType, string itemId, string cartType, string receipt, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addBusinessEventAndAutoFetchReceipt(string currency, int amount, string itemType, string itemId, string cartType, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addResourceEvent(int flowType, string currency, float amount, string itemType, string itemId, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addProgressionEvent(int progressionStatus, string progression01, string progression02, string progression03, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addProgressionEventWithScore(int progressionStatus, string progression01, string progression02, string progression03, int score, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addDesignEvent(string eventId, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addDesignEventWithValue(string eventId, float value, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addErrorEvent(int severity, string message, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addAdEventWithDuration(int adAction, int adType, string adSdkName, string adPlacement, long duration, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addAdEventWithReason(int adAction, int adType, string adSdkName, string adPlacement, int noAdReason, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addAdEvent(int adAction, int adType, string adSdkName, string adPlacement, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void addImpressionEvent(string adNetworkName, string adNetworkVersion, string impressionData, string fields, bool mergeFields);

        [DllImport ("__Internal")]
        private static extern void setEnabledInfoLog(bool enabled);

        [DllImport ("__Internal")]
        private static extern void setEnabledVerboseLog(bool enabled);

        [DllImport ("__Internal")]
        private static extern void setManualSessionHandling(bool enabled);

        [DllImport ("__Internal")]
        private static extern void setEventSubmission(bool enabled);

        [DllImport ("__Internal")]
        private static extern void gameAnalyticsStartSession();

        [DllImport ("__Internal")]
        private static extern void gameAnalyticsEndSession();

        [DllImport ("__Internal")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string getRemoteConfigsValueAsString(string key, string defaultValue);

        [DllImport ("__Internal")]
        private static extern bool isRemoteConfigsReady();

        [DllImport ("__Internal")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string getRemoteConfigsContentAsString();

        [DllImport ("__Internal")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string getABTestingId();

        [DllImport ("__Internal")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string getABTestingVariantId();

        [DllImport ("__Internal")]
        private static extern void startTimer(string key);

        [DllImport ("__Internal")]
        private static extern void pauseTimer(string key);

        [DllImport ("__Internal")]
        private static extern void resumeTimer(string key);

        [DllImport ("__Internal")]
        private static extern long stopTimer(string key);

        private static void initialize(string gamekey, string gamesecret)
        {
            gaInitialize(gamekey, gamesecret, GameAnalytics.SettingsGA.NativeErrorReporting);
        }

#endif
    }
}
