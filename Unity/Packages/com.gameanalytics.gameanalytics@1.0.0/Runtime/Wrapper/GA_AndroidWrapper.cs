using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using GameAnalyticsSDK.Utilities;

namespace GameAnalyticsSDK.Wrapper
{
    public partial class GA_Wrapper
    {
#if (UNITY_ANDROID) && !(UNITY_EDITOR)

        private static readonly AndroidJavaClass GA = new AndroidJavaClass("com.gameanalytics.sdk.GameAnalytics");
        private static readonly AndroidJavaClass UNITY_GA = new AndroidJavaClass("com.gameanalytics.sdk.unity.UnityGameAnalytics");
#if gameanalytics_mopub_enabled
        private static readonly AndroidJavaClass MoPubClass = new AndroidJavaClass("com.mopub.unity.MoPubUnityPlugin");
#endif
#if gameanalytics_topon_enabled
        private static readonly AndroidJavaClass TopOnClass = new AndroidJavaClass("com.anythink.core.api.ATSDK");
#endif

        private static void configureAvailableCustomDimensions01(string list)
        {
            IList<object> iList = GA_MiniJSON.Deserialize(list) as IList<object>;
            ArrayList array = new ArrayList();
            foreach(object entry in iList)
            {
                array.Add(entry);
            }

            GA.CallStatic("configureAvailableCustomDimensions01", array.ToArray(typeof(string)));
        }

        private static void configureAvailableCustomDimensions02(string list)
        {
            IList<object> iList = GA_MiniJSON.Deserialize(list) as IList<object>;
            ArrayList array = new ArrayList();
            foreach(object entry in iList)
            {
                array.Add(entry);
            }
            GA.CallStatic("configureAvailableCustomDimensions02", array.ToArray(typeof(string)));
        }

        private static void configureAvailableCustomDimensions03(string list)
        {
            IList<object> iList = GA_MiniJSON.Deserialize(list) as IList<object>;
            ArrayList array = new ArrayList();
            foreach(object entry in iList)
            {
                array.Add(entry);
            }
            GA.CallStatic("configureAvailableCustomDimensions03", array.ToArray(typeof(string)));
        }

        private static void configureAvailableResourceCurrencies(string list)
        {
            IList<object> iList = GA_MiniJSON.Deserialize(list) as IList<object>;
            ArrayList array = new ArrayList();
            foreach(object entry in iList)
            {
                array.Add(entry);
            }
            GA.CallStatic("configureAvailableResourceCurrencies", array.ToArray(typeof(string)));
        }

        private static void configureAvailableResourceItemTypes(string list)
        {
            IList<object> iList = GA_MiniJSON.Deserialize(list) as IList<object>;
            ArrayList array = new ArrayList();
            foreach(object entry in iList)
            {
                array.Add(entry);
            }
            GA.CallStatic("configureAvailableResourceItemTypes", array.ToArray(typeof(string)));
        }

        private static void configureSdkGameEngineVersion(string unitySdkVersion)
        {
            GA.CallStatic("configureSdkGameEngineVersion", unitySdkVersion);
        }

        private static void configureGameEngineVersion(string unityEngineVersion)
        {
            GA.CallStatic("configureGameEngineVersion", unityEngineVersion);
        }

        private static void configureBuild(string build)
        {
            GA.CallStatic("configureBuild", build);
        }

        private static void configureUserId(string userId)
        {
            GA.CallStatic("configureUserId", userId);
        }

        private static void configureAutoDetectAppVersion(bool flag)
        {
            GA.CallStatic("configureAutoDetectAppVersion", flag);
        }

        private static void initialize(string gamekey, string gamesecret)
        {
            UNITY_GA.CallStatic("initialize");

            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");

            GA.CallStatic("setEnabledErrorReporting", GameAnalytics.SettingsGA.NativeErrorReporting);
            AndroidJavaClass ga = new AndroidJavaClass("com.gameanalytics.sdk.GAPlatform");
            ga.CallStatic("initialize", activity);
            GA.CallStatic("initialize", gamekey, gamesecret);
        }

        private static void setCustomDimension01(string customDimension)
        {
            GA.CallStatic("setCustomDimension01", customDimension);
        }

        private static void setCustomDimension02(string customDimension)
        {
            GA.CallStatic("setCustomDimension02", customDimension);
        }

        private static void setCustomDimension03(string customDimension)
        {
            GA.CallStatic("setCustomDimension03", customDimension);
        }

        private static void setGlobalCustomEventFields(string customFields)
        {
            GA.CallStatic("setGlobalCustomEventFields", customFields);
        }

        private static void addBusinessEvent(string currency, int amount, string itemType, string itemId, string cartType, string fields, bool mergeFields)
        {
            GA.CallStatic("addBusinessEvent", currency, amount, itemType, itemId, cartType, fields, mergeFields);
        }

        private static void addBusinessEventWithReceipt(string currency, int amount, string itemType, string itemId, string cartType, string receipt, string store, string signature, string fields, bool mergeFields)
        {
            GA.CallStatic("addBusinessEvent", currency, amount, itemType, itemId, cartType, receipt, store, signature, fields, mergeFields);
        }

        private static void addResourceEvent(int flowType, string currency, float amount, string itemType, string itemId, string fields, bool mergeFields)
        {
            GA.CallStatic("addResourceEvent", flowType, currency, amount, itemType, itemId, fields, mergeFields);
        }

        private static void addProgressionEvent(int progressionStatus, string progression01, string progression02, string progression03, string fields, bool mergeFields)
        {
            GA.CallStatic("addProgressionEvent", progressionStatus, progression01, progression02, progression03, fields, mergeFields);
        }

        private static void addProgressionEventWithScore(int progressionStatus, string progression01, string progression02, string progression03, int score, string fields, bool mergeFields)
        {
            GA.CallStatic("addProgressionEvent", progressionStatus, progression01, progression02, progression03, (double)score, fields, mergeFields);
        }

        private static void addDesignEvent(string eventId, string fields, bool mergeFields)
        {
            GA.CallStatic("addDesignEvent", eventId, fields, mergeFields);
        }

        private static void addDesignEventWithValue(string eventId, float value, string fields, bool mergeFields)
        {
            GA.CallStatic("addDesignEvent", eventId, (double)value, fields, mergeFields);
        }

        private static void addErrorEvent(int severity, string message, string fields, bool mergeFields)
        {
            GA.CallStatic("addErrorEvent", severity, message, fields, mergeFields);
        }

        private static void addAdEventWithDuration(int adAction, int adType, string adSdkName, string adPlacement, long duration, string fields, bool mergeFields)
        {
            GA.CallStatic("addAdEvent", adAction, adType, adSdkName, adPlacement, duration, fields, mergeFields);
        }

        private static void addAdEventWithReason(int adAction, int adType, string adSdkName, string adPlacement, int noAdReason, string fields, bool mergeFields)
        {
            GA.CallStatic("addAdEvent", adAction, adType, adSdkName, adPlacement, noAdReason, fields, mergeFields);
        }

        private static void addAdEvent(int adAction, int adType, string adSdkName, string adPlacement, string fields, bool mergeFields)
        {
            GA.CallStatic("addAdEvent", adAction, adType, adSdkName, adPlacement, fields, mergeFields);
        }

        private static void setEnabledInfoLog(bool enabled)
        {
            GA.CallStatic("setEnabledInfoLog", enabled);
        }

        private static void setEnabledVerboseLog(bool enabled)
        {
            GA.CallStatic("setEnabledVerboseLog", enabled);
        }

        private static void setManualSessionHandling(bool enabled)
        {
            GA.CallStatic("setEnabledManualSessionHandling", enabled);
        }

        private static void setEventSubmission(bool enabled)
        {
            GA.CallStatic("setEnabledEventSubmission", enabled);
        }

        private static void gameAnalyticsStartSession()
        {
            GA.CallStatic("startSession");
        }

        private static void gameAnalyticsEndSession()
        {
            GA.CallStatic("endSession");
        }

        private static string getRemoteConfigsValueAsString(string key, string defaultValue)
        {
            return GA.CallStatic<string>("getRemoteConfigsValueAsString", key, defaultValue);
        }

        private static bool isRemoteConfigsReady ()
        {
            return GA.CallStatic<bool>("isRemoteConfigsReady");
        }

        private static string getRemoteConfigsContentAsString()
        {
            return GA.CallStatic<string>("getRemoteConfigsContentAsString");
        }

        private static string getABTestingId()
        {
            return GA.CallStatic<string>("getABTestingId");
        }

        private static string getABTestingVariantId()
        {
            return GA.CallStatic<string>("getABTestingVariantId");
        }

        private static void startTimer(string key)
        {
            GA.CallStatic("startTimer", key);
        }

        private static void pauseTimer(string key)
        {
            GA.CallStatic("pauseTimer", key);
        }

        private static void resumeTimer(string key)
        {
            GA.CallStatic("resumeTimer", key);
        }

        private static long stopTimer(string key)
        {
            return GA.CallStatic<long>("stopTimer", key);
        }
#endif
    }
}
