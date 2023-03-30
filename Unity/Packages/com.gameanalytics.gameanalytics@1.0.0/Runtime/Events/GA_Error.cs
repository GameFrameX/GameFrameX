/// <summary>
/// This class handles quality (QA) events, such as crashes, fps, etc.
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using GameAnalyticsSDK.Wrapper;

namespace GameAnalyticsSDK.Events
{
	public static class GA_Error
	{
		#region public methods

		public static void NewEvent(GAErrorSeverity severity, string message, IDictionary<string, object> fields, bool mergeFields)
		{
			CreateNewEvent(severity, message, fields, mergeFields);
		}

		#endregion

		#region private methods

		private static void CreateNewEvent(GAErrorSeverity severity, string message, IDictionary<string, object> fields, bool mergeFields = false)
		{
			GA_Wrapper.AddErrorEvent(severity, message, fields, mergeFields);
		}

		#endregion
	}
}