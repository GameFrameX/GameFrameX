using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace GameAnalyticsSDK.Wrapper
{
    public partial class GA_Wrapper
    {
#if (UNITY_TIZEN) && (!UNITY_EDITOR)

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
		private static extern void initialize(string gamekey, string gamesecret);

		[DllImport ("__Internal")]
		private static extern void setCustomDimension01(string customDimension);

		[DllImport ("__Internal")]
		private static extern void setCustomDimension02(string customDimension);

		[DllImport ("__Internal")]
		private static extern void setCustomDimension03(string customDimension);

		[DllImport ("__Internal")]
		private static extern void setGlobalCustomEventFields(string customFields);

		[DllImport ("__Internal")]
		private static extern void addBusinessEvent(string currency, int amount, string itemType, string itemId, string cartType, string fields, bool mergeFields);

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
		private static extern void setEnabledInfoLog(bool enabled);

		[DllImport ("__Internal")]
		private static extern void setEnabledVerboseLog(bool enabled);

		[DllImport ("__Internal")]
		private static extern void setEnabledManualSessionHandling(bool enabled);

        [DllImport ("__Internal")]
        private static extern void setEnabledEventSubmission(bool enabled);

		[DllImport ("__Internal")]
		private static extern void gameAnalyticsStartSession();

		[DllImport ("__Internal")]
		private static extern void gameAnalyticsEndSession();

        private static void setManualSessionHandling(bool enabled)
        {
            setEnabledManualSessionHandling(enabled);
        }

        private static void setEventSubmission(bool enabled)
        {
            setEnabledEventSubmission(enabled);
        }

		private static string getRemoteConfigsValueAsString(string key, string defaultValue)
		{
			return defaultValue;
		}

		private static bool isRemoteConfigsReady ()
		{
			return false;
		}

		private static string getRemoteConfigsContentAsString()
		{
			return "";
		}

        private static string getABTestingId()
		{
			return "";
		}

        private static string getABTestingVariantId()
		{
			return "";
		}

        private static void configureAutoDetectAppVersion (bool flag)
        {
            // not supported
        }
#endif
	}
}
