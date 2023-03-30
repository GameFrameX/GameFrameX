using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK.Utilities;
using System.Text;
using System.Runtime.InteropServices;

namespace GameAnalyticsSDK.Wrapper
{
    public partial class GA_Wrapper
    {

#if (UNITY_WSA) && (!UNITY_EDITOR)

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void configureAvailableCustomDimensions01UWP([MarshalAs(UnmanagedType.LPWStr)]string list);

        private static void configureAvailableCustomDimensions01(string list)
        {
            configureAvailableCustomDimensions01UWP(list);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void configureAvailableCustomDimensions02UWP([MarshalAs(UnmanagedType.LPWStr)]string list);

        private static void configureAvailableCustomDimensions02(string list)
        {
            configureAvailableCustomDimensions02UWP(list);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void configureAvailableCustomDimensions03UWP([MarshalAs(UnmanagedType.LPWStr)]string list);

        private static void configureAvailableCustomDimensions03(string list)
        {
            configureAvailableCustomDimensions03UWP(list);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void configureAvailableResourceCurrenciesUWP([MarshalAs(UnmanagedType.LPWStr)]string list);

        private static void configureAvailableResourceCurrencies(string list)
        {
            configureAvailableResourceCurrenciesUWP(list);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void configureAvailableResourceItemTypesUWP([MarshalAs(UnmanagedType.LPWStr)]string list);

        private static void configureAvailableResourceItemTypes(string list)
        {
            configureAvailableResourceItemTypesUWP(list);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void configureSdkGameEngineVersionUWP([MarshalAs(UnmanagedType.LPWStr)]string unitySdkVersion);

        private static void configureSdkGameEngineVersion(string unitySdkVersion)
        {
            configureSdkGameEngineVersionUWP(unitySdkVersion);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void configureGameEngineVersionUWP([MarshalAs(UnmanagedType.LPWStr)]string unityEngineVersion);

        private static void configureGameEngineVersion(string unityEngineVersion)
        {
            configureGameEngineVersionUWP(unityEngineVersion);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void configureBuildUWP([MarshalAs(UnmanagedType.LPWStr)]string build);

        private static void configureBuild(string build)
        {
            configureBuildUWP(build);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void configureUserIdUWP([MarshalAs(UnmanagedType.LPWStr)]string userId);

        private static void configureUserId(string userId)
        {
            configureUserIdUWP(userId);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void initializeUWP([MarshalAs(UnmanagedType.LPWStr)]string gamekey, [MarshalAs(UnmanagedType.LPWStr)]string gamesecret);

        private static void initialize(string gamekey, string gamesecret)
        {
            initializeUWP(gamekey, gamesecret);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void setCustomDimension01UWP([MarshalAs(UnmanagedType.LPWStr)]string customDimension);

        private static void setCustomDimension01(string customDimension)
        {
            setCustomDimension01UWP(customDimension);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void setCustomDimension02UWP([MarshalAs(UnmanagedType.LPWStr)]string customDimension);

        private static void setCustomDimension02(string customDimension)
        {
            setCustomDimension02UWP(customDimension);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void setCustomDimension03UWP([MarshalAs(UnmanagedType.LPWStr)]string customDimension);

        private static void setCustomDimension03(string customDimension)
        {
            setCustomDimension03UWP(customDimension);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void setGlobalCustomEventFieldsUWP([MarshalAs(UnmanagedType.LPWStr)]string customfields);

        private static void setGlobalCustomEventFields(string customfields)
        {
            setGlobalCustomEventFieldsUWP(customfields);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void addBusinessEventUWP([MarshalAs(UnmanagedType.LPWStr)]string currency, double amount, [MarshalAs(UnmanagedType.LPWStr)]string itemType, [MarshalAs(UnmanagedType.LPWStr)]string itemId, [MarshalAs(UnmanagedType.LPWStr)]string cartType, [MarshalAs(UnmanagedType.LPWStr)]string fields, double mergeFields);

        private static void addBusinessEvent(string currency, int amount, string itemType, string itemId, string cartType, string fields, bool mergeFields)
        {
            addBusinessEventUWP(currency, amount, itemType, itemId, cartType, fields, mergeFields ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void addResourceEventUWP(double flowType, [MarshalAs(UnmanagedType.LPWStr)]string currency, double amount, [MarshalAs(UnmanagedType.LPWStr)]string itemType, [MarshalAs(UnmanagedType.LPWStr)]string itemId, [MarshalAs(UnmanagedType.LPWStr)]string fields, double mergeFields);

        private static void addResourceEvent(int flowType, string currency, float amount, string itemType, string itemId, string fields, bool mergeFields)
        {
            addResourceEventUWP(flowType, currency, amount, itemType, itemId, fields, mergeFields ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void addProgressionEventUWP(double progressionStatus, [MarshalAs(UnmanagedType.LPWStr)]string progression01, [MarshalAs(UnmanagedType.LPWStr)]string progression02, [MarshalAs(UnmanagedType.LPWStr)]string progression03, [MarshalAs(UnmanagedType.LPWStr)]string fields, double mergeFields);

        private static void addProgressionEvent(int progressionStatus, string progression01, string progression02, string progression03, string fields, bool mergeFields)
        {
            addProgressionEventUWP(progressionStatus, progression01, progression02, progression03, fields, mergeFields ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void addProgressionEventWithScoreUWP(double progressionStatus, [MarshalAs(UnmanagedType.LPWStr)]string progression01, [MarshalAs(UnmanagedType.LPWStr)]string progression02, [MarshalAs(UnmanagedType.LPWStr)]string progression03, double score, [MarshalAs(UnmanagedType.LPWStr)]string fields, double mergeFields);

        private static void addProgressionEventWithScore(int progressionStatus, string progression01, string progression02, string progression03, int score, string fields, bool mergeFields)
        {
            addProgressionEventWithScoreUWP(progressionStatus, progression01, progression02, progression03, score, fields, mergeFields ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void addDesignEventUWP([MarshalAs(UnmanagedType.LPWStr)]string eventId, [MarshalAs(UnmanagedType.LPWStr)]string fields, double mergeFields);

        private static void addDesignEvent(string eventId, string fields, bool mergeFields)
        {
            addDesignEventUWP(eventId, fields, mergeFields ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void addDesignEventWithValueUWP([MarshalAs(UnmanagedType.LPWStr)]string eventId, double value, [MarshalAs(UnmanagedType.LPWStr)]string fields, double mergeFields);

        private static void addDesignEventWithValue(string eventId, float value, string fields, bool mergeFields)
        {
            addDesignEventWithValueUWP(eventId, value, fields, mergeFields ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void addErrorEventUWP(double severity, [MarshalAs(UnmanagedType.LPWStr)]string message, [MarshalAs(UnmanagedType.LPWStr)]string fields, double mergeFields);

        private static void addErrorEvent(int severity, string message, string fields, bool mergeFields)
        {
            addErrorEventUWP(severity, message, fields, mergeFields ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void setEnabledInfoLog(double flag);

        private static void setEnabledInfoLog(bool enabled)
        {
            setEnabledInfoLog(enabled ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void setEnabledVerboseLog(double flag);

        private static void setEnabledVerboseLog(bool enabled)
        {
            setEnabledVerboseLog(enabled ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void setManualSessionHandling(double flag);

        private static void setManualSessionHandling(bool enabled)
        {
            setManualSessionHandling(enabled ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void setEventSubmission(double flag);

        private static void setEventSubmission(bool enabled)
        {
            setEventSubmission(enabled ? 1 : 0);
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void gameAnalyticsStartSession();

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void gameAnalyticsEndSession();

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void getRemoteConfigsValueAsStringWithDefaultValueUWP([MarshalAs(UnmanagedType.LPWStr)]string key, [MarshalAs(UnmanagedType.LPWStr)]string defaultValue, StringBuilder outResult);

        private static string getRemoteConfigsValueAsString(string key, string defaultValue)
        {
            StringBuilder buffer = new StringBuilder(255);
            getRemoteConfigsValueAsStringWithDefaultValueUWP(key, defaultValue, buffer);
            return buffer.ToString();
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern double isRemoteConfigsReady();

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void getRemoteConfigsContentAsStringUWP(StringBuilder outResult);

        private static string getRemoteConfigsContentAsString()
        {
            StringBuilder buffer = new StringBuilder(8192);
            getRemoteConfigsContentAsStringUWP(buffer);
            return buffer.ToString();
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void getABTestingIdUWP(StringBuilder outResult);

        private static string getABTestingId()
        {
            StringBuilder buffer = new StringBuilder(8192);
            getABTestingIdUWP(buffer);
            return buffer.ToString();
        }

        [DllImport ("GameAnalytics.UWP.dll")]
		private static extern void getABTestingVariantIdUWP(StringBuilder outResult);

        private static string getABTestingVariantId()
        {
            StringBuilder buffer = new StringBuilder(8192);
            getABTestingVariantIdUWP(buffer);
            return buffer.ToString();
        }

        private static void configureAutoDetectAppVersion (bool flag)
        {
            // not supported
        }
#endif
    }
}
