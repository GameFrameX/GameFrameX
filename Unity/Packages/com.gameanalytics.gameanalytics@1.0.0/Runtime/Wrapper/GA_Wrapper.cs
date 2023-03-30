using UnityEngine;
using System.Collections;
using GameAnalyticsSDK.Validators;
using System.Collections.Generic;
using GameAnalyticsSDK.Utilities;

namespace GameAnalyticsSDK.Wrapper
{
    public partial class GA_Wrapper
    {
        #if (UNITY_EDITOR || (!UNITY_IOS && !UNITY_ANDROID && !UNITY_TVOS && !UNITY_STANDALONE && !UNITY_WEBGL && !UNITY_WSA && !UNITY_WP_8_1 && !UNITY_TIZEN && !UNITY_SAMSUNGTV))

        private static void configureAvailableCustomDimensions01 (string list)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setAvailableCustomDimensions01(" + list + ")");
            }
        }

        private static void configureAvailableCustomDimensions02 (string list)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setAvailableCustomDimensions02(" + list + ")");
            }
        }

        private static void configureAvailableCustomDimensions03 (string list)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setAvailableCustomDimensions03(" + list + ")");
            }
        }

        private static void configureAvailableResourceCurrencies (string list)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setAvailableResourceCurrencies(" + list + ")");
            }
        }

        private static void configureAvailableResourceItemTypes (string list)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setAvailableResourceItemTypes(" + list + ")");
            }
        }

        private static void configureSdkGameEngineVersion (string unitySdkVersion)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("GameAnalytics SDK version: " + unitySdkVersion);
            }
        }

        private static void configureGameEngineVersion (string unityEngineVersion)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                //Debug.Log ("configureGameEngineVersion(" + unityEngineVersion + ")");
            }
        }

        private static void configureBuild (string build)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setBuild(" + build + ")");
            }
        }

        private static void configureUserId (string userId)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("configureUserId(" + userId + ")");
            }
        }

        private static void configureAutoDetectAppVersion (bool flag)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("configureAutoDetectAppVersion(" + flag + ")");
            }
        }

        private static void initialize (string gamekey, string gamesecret)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("initialize(" + gamekey + "," + gamesecret + ")");
            }
        }

        private static void setCustomDimension01 (string customDimension)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setCustomDimension01(" + customDimension + ")");
            }
        }

        private static void setCustomDimension02 (string customDimension)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setCustomDimension02(" + customDimension + ")");
            }
        }

        private static void setCustomDimension03 (string customDimension)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setCustomDimension03(" + customDimension + ")");
            }
        }

        private static void setGlobalCustomEventFields(string customFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor)
            {
                Debug.Log("setGlobalCustomEventFields(" + customFields + ")");
            }
        }

#if UNITY_IOS || UNITY_TVOS
        private static void addBusinessEvent(string currency, int amount, string itemType, string itemId, string cartType, string receipt, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor)
            {
                Debug.Log("addBusinessEvent("+currency+","+amount+","+itemType+","+itemId+","+cartType+","+receipt+")");
            }
        }

        private static void addBusinessEventAndAutoFetchReceipt(string currency, int amount, string itemType, string itemId, string cartType, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor)
            {
                Debug.Log("addBusinessEventAndAutoFetchReceipt("+currency+","+amount+","+itemType+","+itemId+","+cartType+")");
            }
        }




#elif UNITY_ANDROID
        private static void addBusinessEventWithReceipt(string currency, int amount, string itemType, string itemId, string cartType, string receipt, string store, string signature, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor)
            {
                Debug.Log("addBusinessEventWithReceipt("+currency+","+amount+","+itemType+","+itemId+","+cartType+","+receipt+","+store+","+signature+")");
            }
        }
#endif

#if !UNITY_IOS && !UNITY_TVOS
        private static void addBusinessEvent (string currency, int amount, string itemType, string itemId, string cartType, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("addBusinessEvent(" + currency + "," + amount + "," + itemType + "," + itemId + "," + cartType + ")");
            }
        }
        #endif

        private static void addResourceEvent (int flowType, string currency, float amount, string itemType, string itemId, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("addResourceEvent(" + flowType + "," + currency + "," + amount + "," + itemType + "," + itemId + ")");
            }
        }

        private static void addProgressionEvent (int progressionStatus, string progression01, string progression02, string progression03, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("addProgressionEvent(" + progressionStatus + "," + progression01 + "," + progression02 + "," + progression03 + ")");
            }
        }

        private static void addProgressionEventWithScore (int progressionStatus, string progression01, string progression02, string progression03, int score, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("addProgressionEvent(" + progressionStatus + "," + progression01 + "," + progression02 + "," + progression03 + "," + score + ")");
            }
        }

        private static void addDesignEvent (string eventId, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("addDesignEvent(" + eventId + ")");
            }
        }

        private static void addDesignEventWithValue (string eventId, float value, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("addDesignEventWithValue(" + eventId + "," + value + ")");
            }
        }

        private static void addErrorEvent (int severity, string message, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("addErrorEvent(" + severity + "," + message + ")");
            }
        }

        private static void addAdEventWithDuration(int adAction, int adType, string adSdkName, string adPlacement, long duration, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor)
            {
                Debug.Log("addAdEvent(" + adAction + "," + adType + "," + adSdkName + "," + adPlacement + "," + duration + ")");
            }
        }

        private static void addAdEventWithReason(int adAction, int adType, string adSdkName, string adPlacement, int noAdReason, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor)
            {
                Debug.Log("addAdEvent(" + adAction + "," + adType + "," + adSdkName + "," + adPlacement + "," + noAdReason + ")");
            }
        }

        private static void addAdEvent(int adAction, int adType, string adSdkName, string adPlacement, string fields, bool mergeFields)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor)
            {
                Debug.Log("addAdEvent(" + adAction + "," + adType + "," + adSdkName + "," + adPlacement + ")");
            }
        }

        private static void setEnabledInfoLog (bool enabled)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("GameAnalytics setInfoLog(" + enabled + ")\nInfo logs can be deactivated in the Advanced section of the Settings object.");
            }
        }

        private static void setEnabledVerboseLog (bool enabled)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("GameAnalytics setVerboseLog(" + enabled + ")");
            }
        }

        private static void setManualSessionHandling (bool enabled)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setManualSessionHandling(" + enabled + ")");
            }
        }

        private static void setEventSubmission (bool enabled)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("setEventSubmission(" + enabled + ")");
            }
        }

        private static void setUsePlayerSettingsBundleVersionForBuild (bool enabled)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("Using Player Settings bundle version for build(" + enabled + ")");
            }
        }

        private static void gameAnalyticsStartSession ()
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("gameAnalyticsStartSession()");
            }
        }

        private static void gameAnalyticsEndSession ()
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("gameAnalyticsEndSession()");
            }
        }

        // ----------------------- REMOTE CONFIGS ---------------------- //
        private static string getRemoteConfigsValueAsString(string key, string defaultValue)
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("getRemoteConfigsValueAsString()");
            }
            return defaultValue;
        }

        private static bool isRemoteConfigsReady ()
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("isRemoteConfigsReady()");
            }
            return false;
        }

        private static string getRemoteConfigsContentAsString()
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor) {
                Debug.Log ("getRemoteConfigsContentAsString()");
            }
            return "";
        }

        private static string getABTestingId()
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor)
            {
                Debug.Log("getABTestingId()");
            }
            return "";
        }

        private static string getABTestingVariantId()
        {
            if (GameAnalytics.SettingsGA.InfoLogEditor)
            {
                Debug.Log("getABTestingVariantId()");
            }
            return "";
        }

        private static void startTimer(string key)
        {
            Debug.Log("startTimer(" + key + ")");
        }

        public static void pauseTimer(string key)
        {
            Debug.Log("pauseTimer(" + key + ")");
        }

        public static void resumeTimer(string key)
        {
            Debug.Log("resumeTimer(" + key + ")");
        }

        public static long stopTimer(string key)
        {
            Debug.Log("stopTimer(" + key + ")");
            return 0;
        }

#endif

        public static void SetAvailableCustomDimensions01 (string list)
        {
            configureAvailableCustomDimensions01 (list);
        }

        public static void SetAvailableCustomDimensions02 (string list)
        {
            configureAvailableCustomDimensions02 (list);
        }

        public static void SetAvailableCustomDimensions03 (string list)
        {
            configureAvailableCustomDimensions03 (list);
        }

        public static void SetAvailableResourceCurrencies (string list)
        {
            configureAvailableResourceCurrencies (list);
        }

        public static void SetAvailableResourceItemTypes (string list)
        {
            configureAvailableResourceItemTypes (list);
        }

        public static void SetUnitySdkVersion (string unitySdkVersion)
        {
            configureSdkGameEngineVersion (unitySdkVersion);
        }

        public static void SetUnityEngineVersion (string unityEngineVersion)
        {
            configureGameEngineVersion (unityEngineVersion);
        }

        public static void SetBuild (string build)
        {
#if UNITY_EDITOR
            if (GAValidator.ValidateBuild (build)) {
                configureBuild (build);
            }
#else
                configureBuild (build);
#endif
        }

        public static void SetCustomUserId (string userId)
        {
#if UNITY_EDITOR
            if (GAValidator.ValidateUserId (userId)) {
                configureUserId (userId);
            }
#else
                configureUserId (userId);
#endif
        }

        public static void SetEnabledManualSessionHandling (bool enabled)
        {
            setManualSessionHandling (enabled);
        }

        public static void SetEnabledEventSubmission (bool enabled)
        {
            setEventSubmission (enabled);
        }

        public static void SetAutoDetectAppVersion (bool flag)
        {
            configureAutoDetectAppVersion (flag);
        }

        public static void StartSession ()
        {
            if (GameAnalyticsSDK.State.GAState.IsManualSessionHandlingEnabled()) {
                gameAnalyticsStartSession ();
            } else {
                Debug.Log ("Manual session handling is not enabled. \nPlease check the \"Use manual session handling\" option in the \"Advanced\" section of the Settings object.");
            }
        }

        public static void EndSession ()
        {
            if (GameAnalyticsSDK.State.GAState.IsManualSessionHandlingEnabled()) {
                gameAnalyticsEndSession ();
            } else {
                Debug.Log ("Manual session handling is not enabled. \nPlease check the \"Use manual session handling\" option in the \"Advanced\" section of the Settings object.");
            }
        }

        public static void Initialize (string gamekey, string gamesecret)
        {
#if UNITY_EDITOR
            if (GAValidator.ValidateKeys (gamekey, gamesecret)) {
                initialize (gamekey, gamesecret);
            }
#else
                initialize (gamekey, gamesecret);
#endif
        }

        public static void SetCustomDimension01 (string customDimension)
        {
            setCustomDimension01 (customDimension);
        }

        public static void SetCustomDimension02 (string customDimension)
        {

            setCustomDimension02 (customDimension);
        }

        public static void SetCustomDimension03 (string customDimension)
        {
            setCustomDimension03 (customDimension);
        }

        public static void SetGlobalCustomEventFields(IDictionary<string, object> customFields)
        {
            string fieldsAsString = DictionaryToJsonString(customFields);
            setGlobalCustomEventFields(fieldsAsString);
        }

#if UNITY_IOS || UNITY_TVOS
        public static void AddBusinessEvent(string currency, int amount, string itemType, string itemId, string cartType, string receipt, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
            addBusinessEvent(currency, amount, itemType, itemId, cartType, receipt, fieldsAsString, mergeFields);
        }

        public static void AddBusinessEventAndAutoFetchReceipt(string currency, int amount, string itemType, string itemId, string cartType, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
            addBusinessEventAndAutoFetchReceipt(currency, amount, itemType, itemId, cartType, fieldsAsString, mergeFields);
        }


#elif UNITY_ANDROID
        public static void AddBusinessEventWithReceipt(string currency, int amount, string itemType, string itemId, string cartType, string receipt, string store, string signature, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
            addBusinessEventWithReceipt(currency, amount, itemType, itemId, cartType, receipt, store, signature, fieldsAsString, mergeFields);
        }
#endif

#if !UNITY_IOS && !UNITY_TVOS
        public static void AddBusinessEvent (string currency, int amount, string itemType, string itemId, string cartType, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
#if UNITY_EDITOR
            if (GAValidator.ValidateBusinessEvent (currency, amount, cartType, itemType, itemId)) {
                addBusinessEvent (currency, amount, itemType, itemId, cartType, fieldsAsString, mergeFields);
            }
#else
                addBusinessEvent (currency, amount, itemType, itemId, cartType, fieldsAsString, mergeFields);
#endif
        }
#endif

        public static void AddResourceEvent (GAResourceFlowType flowType, string currency, float amount, string itemType, string itemId, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
#if UNITY_EDITOR
            if (GAValidator.ValidateResourceEvent (flowType, currency, amount, itemType, itemId)) {
                addResourceEvent ((int)flowType, currency, amount, itemType, itemId, fieldsAsString, mergeFields);
            }
#else
                addResourceEvent ((int)flowType, currency, amount, itemType, itemId, fieldsAsString, mergeFields);
#endif
        }

        public static void AddProgressionEvent (GAProgressionStatus progressionStatus, string progression01, string progression02, string progression03, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
#if UNITY_EDITOR
            if (GAValidator.ValidateProgressionEvent (progressionStatus, progression01, progression02, progression03)) {
                addProgressionEvent ((int)progressionStatus, progression01, progression02, progression03, fieldsAsString, mergeFields);
            }
#else
                addProgressionEvent ((int)progressionStatus, progression01, progression02, progression03, fieldsAsString, mergeFields);
#endif
        }

        public static void AddProgressionEventWithScore (GAProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
#if UNITY_EDITOR
            if (GAValidator.ValidateProgressionEvent (progressionStatus, progression01, progression02, progression03)) {
                addProgressionEventWithScore ((int)progressionStatus, progression01, progression02, progression03, score, fieldsAsString, mergeFields);
            }
#else
                addProgressionEventWithScore ((int)progressionStatus, progression01, progression02, progression03, score, fieldsAsString, mergeFields);
#endif
        }

        public static void AddDesignEvent (string eventID, float eventValue, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
#if UNITY_EDITOR
            if (GAValidator.ValidateDesignEvent (eventID)) {
                addDesignEventWithValue (eventID, eventValue, fieldsAsString, mergeFields);
            }
#else
                addDesignEventWithValue (eventID, eventValue, fieldsAsString, mergeFields);
#endif
        }

        public static void AddDesignEvent (string eventID, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
#if UNITY_EDITOR
            if (GAValidator.ValidateDesignEvent (eventID)) {
                addDesignEvent (eventID, fieldsAsString, mergeFields);
            }
#else
                addDesignEvent (eventID, fieldsAsString, mergeFields);
#endif
        }

        public static void AddErrorEvent (GAErrorSeverity severity, string message, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
#if UNITY_EDITOR
            if (GAValidator.ValidateErrorEvent(severity,message)) {
                addErrorEvent ((int)severity, message, fieldsAsString, mergeFields);
            }
#else
                addErrorEvent ((int)severity, message, fieldsAsString, mergeFields);
#endif
        }

        public static void AddAdEventWithDuration(GAAdAction adAction, GAAdType adType, string adSdkName, string adPlacement, long duration, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
#if UNITY_EDITOR
            if (GAValidator.ValidateAdEvent(adAction, adType, adSdkName, adPlacement))
            {
                addAdEventWithDuration((int)adAction, (int)adType, adSdkName, adPlacement, duration, fieldsAsString, mergeFields);
            }
#elif UNITY_IOS || UNITY_ANDROID
                addAdEventWithDuration((int)adAction, (int)adType, adSdkName, adPlacement, duration, fieldsAsString, mergeFields);
#endif
        }

        public static void AddAdEventWithReason(GAAdAction adAction, GAAdType adType, string adSdkName, string adPlacement, GAAdError noAdReason, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
#if UNITY_EDITOR
            if (GAValidator.ValidateAdEvent(adAction, adType, adSdkName, adPlacement))
            {
                addAdEventWithReason((int)adAction, (int)adType, adSdkName, adPlacement, (int)noAdReason, fieldsAsString, mergeFields);
            }
#elif UNITY_IOS || UNITY_ANDROID
                addAdEventWithReason((int)adAction, (int)adType, adSdkName, adPlacement, (int)noAdReason, fieldsAsString, mergeFields);
#endif
        }

        public static void AddAdEvent(GAAdAction adAction, GAAdType adType, string adSdkName, string adPlacement, IDictionary<string, object> fields, bool mergeFields)
        {
            string fieldsAsString = DictionaryToJsonString(fields);
#if UNITY_EDITOR
            if (GAValidator.ValidateAdEvent(adAction, adType, adSdkName, adPlacement))
            {
                addAdEvent((int)adAction, (int)adType, adSdkName, adPlacement, fieldsAsString, mergeFields);
            }
#elif UNITY_IOS || UNITY_ANDROID
                addAdEvent((int)adAction, (int)adType, adSdkName, adPlacement, fieldsAsString, mergeFields);
#endif
        }

        public static void SetInfoLog (bool enabled)
        {
            setEnabledInfoLog (enabled);
        }

        public static void SetVerboseLog (bool enabled)
        {
            setEnabledVerboseLog (enabled);
        }

        // ----------------------- REMOTE CONFIGS ---------------------- //
        public static string GetRemoteConfigsValueAsString(string key, string defaultValue)
        {
            return getRemoteConfigsValueAsString(key, defaultValue);
        }

        public static bool IsRemoteConfigsReady()
        {
#if (UNITY_WSA) && (!UNITY_EDITOR)
            return isRemoteConfigsReady() != 0;
#else
            return isRemoteConfigsReady();
#endif
        }

        public static string GetRemoteConfigsContentAsString()
        {
            return getRemoteConfigsContentAsString();
        }

        public static string GetABTestingId()
        {
            return getABTestingId();
        }

        public static string GetABTestingVariantId()
        {
            return getABTestingVariantId();
        }

        private static string DictionaryToJsonString(IDictionary<string, object> dict)
        {
            Hashtable table = new Hashtable();
            if (dict != null)
            {
                foreach (KeyValuePair<string, object> pair in dict)
                {
                    table.Add(pair.Key, pair.Value);
                }
            }
            return GA_MiniJSON.Serialize(table);
        }

        // TIMER FUNCTIONS
        public static void StartTimer(string key)
        {
#if UNITY_EDITOR
            startTimer(key);
#elif UNITY_IOS || UNITY_ANDROID
            startTimer(key);
#endif
        }

        public static void PauseTimer(string key)
        {
#if UNITY_EDITOR
            pauseTimer(key);
#elif UNITY_IOS || UNITY_ANDROID
            pauseTimer(key);
#endif
        }

        public static void ResumeTimer(string key)
        {
#if UNITY_EDITOR
            resumeTimer(key);
#elif UNITY_IOS || UNITY_ANDROID
            resumeTimer(key);
#endif
        }

        public static long StopTimer(string key)
        {
#if UNITY_EDITOR
            return stopTimer(key);
#elif UNITY_IOS || UNITY_ANDROID
            return stopTimer(key);
#else
            return 0;
#endif
        }
    }
}
