// This class handles game design events, such as kills, deaths, high scores, etc.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK.Wrapper;

namespace GameAnalyticsSDK.Events
{
	public static class GA_Design
	{
        #region public methods

        /// <summary>
        /// Creates a new event
        /// </summary>
        /// <param name='eventName'>
        /// A event string you define
        /// </param>
        /// <param name='eventValue'>
        /// A value of the event
        /// </param>
        /// <param name="fields">Custom fields.</param>
        public static void NewEvent(string eventName, float eventValue, IDictionary<string, object> fields, bool mergeFields)
		{
			CreateNewEvent(eventName, eventValue, fields, mergeFields);
		}

        /// <summary>
        /// Creates a new event
        /// </summary>
        /// <param name='eventName'>
        /// A event string you define
        /// </param>
        /// <param name="fields">Custom fields.</param>
        public static void NewEvent(string eventName, IDictionary<string, object> fields, bool mergeFields)
		{
			CreateNewEvent(eventName, null, fields, mergeFields);
		}

        #endregion

        #region private methods

        /// <summary>
        /// Adds a custom event to the submit queue (see GA_Queue)
        /// </summary>
        /// <param name="eventName">
        /// Identifies the event so this should be as descriptive as possible. PickedUpAmmo might be a good event name. EventTwo is a bad event name! <see cref="System.String"/>
        /// </param>
        /// <param name="eventValue">
        /// A value relevant to the event. F.x. if the player picks up some shotgun ammo the eventName could be "PickedUpAmmo" and this value could be "Shotgun". This can be null <see cref="System.Nullable<System.Single>"/>
        /// </param>
        /// <param name="fields">Custom fields.</param>
        private static void CreateNewEvent(string eventName, float? eventValue, IDictionary<string, object> fields, bool mergeFields)
		{
			if(eventValue.HasValue)
			{
				GA_Wrapper.AddDesignEvent(eventName, eventValue.Value, fields, mergeFields);
			}
			else
			{
				GA_Wrapper.AddDesignEvent(eventName, fields, mergeFields);
			}
		}

		#endregion
	}
}