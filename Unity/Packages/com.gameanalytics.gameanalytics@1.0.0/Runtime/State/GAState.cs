using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK.Events;
using GameAnalyticsSDK.Setup;
using GameAnalyticsSDK.Wrapper;
using System;

namespace GameAnalyticsSDK.State
{
	
	internal static class GAState
	{

		private static GameAnalyticsSDK.Setup.Settings _settings;

		public static void Init ()
		{
			try {
				_settings = (GameAnalyticsSDK.Setup.Settings)Resources.Load ("GameAnalytics/Settings", typeof(GameAnalyticsSDK.Setup.Settings));
			} catch (Exception ex) {
				Debug.Log ("Could not get Settings during event validation \n" + ex.ToString ());
			}

		}

		private static bool ListContainsString (List<string> _list, string _string)
		{
			if (_list.Contains (_string))
				return true;
			return false;
		}


		#region Public methods

		public static bool IsManualSessionHandlingEnabled()
		{
			return _settings.UseManualSessionHandling;
		}

		public static bool HasAvailableResourceCurrency (string _currency)
		{
			if (ListContainsString (_settings.ResourceCurrencies, _currency))
				return true;
			return false;
		}

		public static bool HasAvailableResourceItemType(string _itemType)
		{
			if (ListContainsString (_settings.ResourceItemTypes, _itemType))
				return true;
			return false;
		}

		public static bool HasAvailableCustomDimensions01(string _dimension01)
		{
			if (ListContainsString (_settings.CustomDimensions01, _dimension01))
				return true;
			return false;
		}

		public static bool HasAvailableCustomDimensions02(string _dimension02)
		{
			if (ListContainsString (_settings.CustomDimensions02, _dimension02)) {
				return true;
			}
			return false;
		}

		public static bool HasAvailableCustomDimensions03(string _dimension03)
		{
			if (ListContainsString (_settings.CustomDimensions03, _dimension03))
				return true;
			return false;
		}

		#endregion

	}
}
