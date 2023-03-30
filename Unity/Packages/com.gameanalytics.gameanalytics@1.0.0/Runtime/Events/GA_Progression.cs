using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK.Wrapper;

namespace GameAnalyticsSDK.Events
{
	public static class GA_Progression
	{
		#region public methods

		public static void NewEvent(GAProgressionStatus progressionStatus, string progression01, IDictionary<string, object> fields, bool mergeFields)
		{
			CreateEvent(progressionStatus, progression01, null, null, null, fields, mergeFields);
		}

		public static void NewEvent(GAProgressionStatus progressionStatus, string progression01, string progression02, IDictionary<string, object> fields, bool mergeFields)
		{
			CreateEvent(progressionStatus, progression01, progression02, null, null, fields, mergeFields);
		}

		public static void NewEvent(GAProgressionStatus progressionStatus, string progression01, string progression02, string progression03, IDictionary<string, object> fields, bool mergeFields)
		{
			CreateEvent(progressionStatus, progression01, progression02, progression03, null, fields, mergeFields);
		}

		public static void NewEvent(GAProgressionStatus progressionStatus, string progression01, int score, IDictionary<string, object> fields, bool mergeFields)
		{
			CreateEvent(progressionStatus, progression01, null, null, score, fields, mergeFields);
		}

		public static void NewEvent(GAProgressionStatus progressionStatus, string progression01, string progression02, int score, IDictionary<string, object> fields, bool mergeFields)
		{
			CreateEvent(progressionStatus, progression01, progression02, null, score, fields, mergeFields);
		}

		public static void NewEvent(GAProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score, IDictionary<string, object> fields, bool mergeFields)
		{
			CreateEvent(progressionStatus, progression01, progression02, progression03, score, fields, mergeFields);
		}

		#endregion

		#region private methods

		private static void CreateEvent(GAProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int? score, IDictionary<string, object> fields, bool mergeFields)
		{
			if(score.HasValue)
			{
				GA_Wrapper.AddProgressionEventWithScore(progressionStatus, progression01, progression02, progression03, score.Value, fields, mergeFields);
			}
			else
			{
				GA_Wrapper.AddProgressionEvent(progressionStatus, progression01, progression02, progression03, fields, mergeFields);
			}
		}

		#endregion
	}
}