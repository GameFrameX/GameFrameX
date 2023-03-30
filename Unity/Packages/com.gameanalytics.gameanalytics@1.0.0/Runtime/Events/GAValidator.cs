using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameAnalyticsSDK.State;

namespace GameAnalyticsSDK.Validators
{
	internal static class GAValidator
	{
		#region Public methods

		/// <summary>
		/// Check if a string matches a defined pattern
		/// </summary>
		/// <returns><c>true</c>, if match <c>false</c> otherwise.</returns>
		/// <param name="s">Given string</param>
		/// <param name="pattern">Pattern.</param>
		public static bool StringMatch(string s, string pattern)
		{
			if(s == null || pattern == null)
			{
				return false;
			}

			return Regex.IsMatch(s, pattern);
		}

		public static bool ValidateBusinessEvent(string currency, int amount, string cartType, string itemType, string itemId)
		{
			// validate currency
			if (!ValidateCurrency(currency))
			{
				Debug.Log("Validation fail - business event - currency: Cannot be (null) and need to be A-Z, 3 characters and in the standard at openexchangerates.org. Failed currency: " + currency);
				return false;
			}

			// do not validate amount - integer is never null !

			// validate cartType
			if (!ValidateShortString(cartType, true))
			{
				Debug.Log("Validation fail - business event - cartType. Cannot be above 32 length. String: " + cartType);
				return false;
			}

			// validate itemType length
			if (!ValidateEventPartLength(itemType, false))
			{
				Debug.Log("Validation fail - business event - itemType: Cannot be (null), empty or above 64 characters. String: " + itemType);
				return false;
			}

			// validate itemType chars
			if (!ValidateEventPartCharacters(itemType))
			{
				Debug.Log("Validation fail - business event - itemType: Cannot contain other characters than A-z, 0-9, -_. ()!?. String: " + itemType);
				return false;
			}

			// validate itemId
			if (!ValidateEventPartLength(itemId, false))
			{
				Debug.Log("Validation fail - business event - itemId. Cannot be (null), empty or above 64 characters. String: " + itemId);
				return false;
			}

			if (!ValidateEventPartCharacters(itemId))
			{
				Debug.Log("Validation fail - business event - itemId: Cannot contain other characters than A-z, 0-9, -_., ()!?. String: " + itemId);
				return false;
			}

			return true;
		}

		public static bool ValidateResourceEvent(GAResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
		{
			if (string.IsNullOrEmpty(currency))
			{
				Debug.Log("Validation fail - resource event - currency: Cannot be (null)");
				return false;
			}

			if (flowType == GAResourceFlowType.Undefined) {
				Debug.Log("Validation fail - resource event - flowType: Invalid flowType");
			}

			if (!GAState.HasAvailableResourceCurrency(currency))
			{
				Debug.Log("Validation fail - resource event - currency: Not found in list of pre-defined resource currencies. String: " + currency);
				return false;
			}
			if (!(amount > 0))
			{
				Debug.Log("Validation fail - resource event - amount: Float amount cannot be 0 or negative. Value: " + amount);
				return false;
			}
			if (string.IsNullOrEmpty(itemType))
			{
				Debug.Log("Validation fail - resource event - itemType: Cannot be (null)");
				return false;
			}
			if (!ValidateEventPartLength(itemType, false))
			{
				Debug.Log("Validation fail - resource event - itemType: Cannot be (null), empty or above 64 characters. String: " + itemType);
				return false;
			}
			if (!ValidateEventPartCharacters(itemType))
			{
				Debug.Log("Validation fail - resource event - itemType: Cannot contain other characters than A-z, 0-9, -_. ()!?. String: " + itemType);
				return false;
			}
			if (!GAState.HasAvailableResourceItemType(itemType))
			{
				Debug.Log("Validation fail - resource event - itemType: Not found in list of pre-defined available resource itemTypes. String: " + itemType);
				return false;
			}
			if (!ValidateEventPartLength(itemId, false))
			{
				Debug.Log("Validation fail - resource event - itemId: Cannot be (null), empty or above 64 characters. String: " + itemId);
				return false;
			}
			if (!ValidateEventPartCharacters(itemId))
			{
				Debug.Log("Validation fail - resource event - itemId: Cannot contain other characters than A-z, 0-9, -_., ()!?. String: " + itemId);
				return false;
			}
			return true;
		}

		public static bool ValidateProgressionEvent(GAProgressionStatus progressionStatus, string progression01, string progression02, string progression03)
		{
			if (progressionStatus == GAProgressionStatus.Undefined)
			{
				Debug.Log("Validation fail - progression event: Invalid progression status.");
				return false;
			}

			// Make sure progressions are defined as either 01, 01+02 or 01+02+03
			if (!string.IsNullOrEmpty(progression03) && !(!string.IsNullOrEmpty(progression02) || string.IsNullOrEmpty(progression01)))
			{
				Debug.Log("Validation fail - progression event: 03 found but 01+02 are invalid. Progression must be set as either 01, 01+02 or 01+02+03.");
				return false;
			}
			else if (!string.IsNullOrEmpty(progression02) && string.IsNullOrEmpty(progression01))
			{
				Debug.Log("Validation fail - progression event: 02 found but not 01. Progression must be set as either 01, 01+02 or 01+02+03");
				return false;
			}
			else if (string.IsNullOrEmpty(progression01))
			{
				Debug.Log("Validation fail - progression event: progression01 not valid. Progressions must be set as either 01, 01+02 or 01+02+03");
				return false;
			}

			// progression01 (required)
			if (!ValidateEventPartLength(progression01, false))
			{
				Debug.Log("Validation fail - progression event - progression01: Cannot be (null), empty or above 64 characters. String: " + progression01);
				return false;
			}
			if (!ValidateEventPartCharacters(progression01))
			{
				Debug.Log("Validation fail - progression event - progression01: Cannot contain other characters than A-z, 0-9, -_. ()!?. String: " + progression01);
				return false;
			}
			// progression02
			if (!string.IsNullOrEmpty(progression02))
			{
				if (!ValidateEventPartLength(progression02, true))
				{
					Debug.Log("Validation fail - progression event - progression02: Cannot be empty or above 64 characters. String: " + progression02);
					return false;
				}
				if (!ValidateEventPartCharacters(progression02))
				{
					Debug.Log("Validation fail - progression event - progression02: Cannot contain other characters than A-z, 0-9, -_. ()!?. String: " + progression02);
					return false;
				}
			}
			// progression03
			if (!string.IsNullOrEmpty(progression03))
			{
				if (!ValidateEventPartLength(progression03, true))
				{
					Debug.Log("Validation fail - progression event - progression03: Cannot be empty or above 64 characters. String: " + progression03);
					return false;
				}
				if (!ValidateEventPartCharacters(progression03))
				{
					Debug.Log("Validation fail - progression event - progression03: Cannot contain other characters than A-z, 0-9, -_. ()!?. String: " + progression03);
					return false;
				}
			}
			return true;
		}

		public static bool ValidateDesignEvent(string eventId)
		{
			if (!ValidateEventIdLength(eventId))
			{
				Debug.Log("Validation fail - design event - eventId: Cannot be (null) or empty. Only 5 event parts allowed seperated by :. Each part need to be 32 characters or less. String: " + eventId);
				return false;
			}
			if (!ValidateEventIdCharacters(eventId))
			{
				Debug.Log("Validation fail - design event - eventId: Non valid characters. Only allowed A-z, 0-9, -_. ()!?. String: " + eventId);
				return false;
			}
			// value: allow 0, negative and nil (not required)
			return true;
		}

		public static bool ValidateErrorEvent(GAErrorSeverity severity, string message)
		{
			if (severity == GAErrorSeverity.Undefined)
			{
				Debug.Log("Validation fail - error event - severity: Severity was unsupported value.");
				return false;
			}
			if (!ValidateLongString(message, true))
			{
				Debug.Log("Validation fail - error event - message: Message cannot be above 8192 characters.");
				return false;
			}
			return true;
		}

        public static bool ValidateAdEvent(GAAdAction adAction, GAAdType adType, string adSdkName, string adPlacement)
        {
            if (adAction == GAAdAction.Undefined)
            {
                Debug.Log("Validation fail - ad event - adAction: Ad action was unsupported value.");
                return false;
            }
            if (adType == GAAdType.Undefined)
            {
                Debug.Log("Validation fail - ad event - adType: Ad type was unsupported value.");
                return false;
            }
            if (!ValidateShortString(adSdkName, false))
            {
                Debug.Log("Validation fail - ad event - message: Ad SDK name cannot be above 32 characters.");
                return false;
            }
            if (!ValidateString(adPlacement, false))
            {
                Debug.Log("Validation fail - ad event - message: Ad placement cannot be above 64 characters.");
                return false;
            }
            return true;
        }

        public static bool ValidateSdkErrorEvent(string gameKey, string gameSecret, GAErrorSeverity type)
		{
			if(!ValidateKeys(gameKey, gameSecret))
			{
				return false;
			}

			if (type == GAErrorSeverity.Undefined)
			{
				Debug.Log("Validation fail - sdk error event - type: Type was unsupported value.");
				return false;
			}
			return true;
		}

		public static bool ValidateKeys(string gameKey, string gameSecret)
		{
			if (StringMatch(gameKey, "^[A-z0-9]{32}$"))
			{
				if (StringMatch(gameSecret, "^[A-z0-9]{40}$"))
				{
					return true;
				}
			}
			return false;
		}

		public static bool ValidateCurrency(string currency)
		{
			if (string.IsNullOrEmpty(currency))
			{
				return false;
			}
			if (!StringMatch(currency, "^[A-Z]{3}$"))
			{
				return false;
			}
			return true;
		}

		public static bool ValidateEventPartLength(string eventPart, bool allowNull)
		{
			if (allowNull == true && string.IsNullOrEmpty(eventPart))
			{
				return true;
			}

			if (string.IsNullOrEmpty(eventPart))
			{
				return false;
			}

			if (eventPart.Length > 64)
			{
				return false;
			}
			return true;
		}

		public static bool ValidateEventPartCharacters(string eventPart)
		{
			if (!StringMatch(eventPart, "^[A-Za-z0-9\\s\\-_\\.\\(\\)\\!\\?]{1,64}$"))
			{
				return false;
			}
			return true;
		}

		public static bool ValidateEventIdLength(string eventId)
		{
			if (string.IsNullOrEmpty(eventId))
			{
				return false;
			}

			if (!StringMatch(eventId, "^[^:]{1,64}(?::[^:]{1,64}){0,4}$"))
			{
				return false;
			}
			return true;
		}

		public static bool ValidateEventIdCharacters(string eventId)
		{
			if (string.IsNullOrEmpty(eventId))
			{
				return false;
			}

			if (!StringMatch(eventId, "^[A-Za-z0-9\\s\\-_\\.\\(\\)\\!\\?]{1,64}(:[A-Za-z0-9\\s\\-_\\.\\(\\)\\!\\?]{1,64}){0,4}$"))
			{
				return false;
			}
			return true;
		}

		public static bool ValidateBuild(string build)
		{
			if (!ValidateShortString(build, false))
			{
				return false;
			}
			return true;
		}

		public static bool ValidateUserId(string uId)
		{
			if (!ValidateString(uId, false))
			{
				Debug.Log("Validation fail - user id: id cannot be (null), empty or above 64 characters.");
				return false;
			}
			return true;
		}

		public static bool ValidateShortString(string shortString, bool canBeEmpty)
		{
			// String is allowed to be empty or nil
			if (canBeEmpty && string.IsNullOrEmpty(shortString))
			{
				return true;
			}

			if (string.IsNullOrEmpty(shortString) || shortString.Length > 32)
			{
				return false;
			}
			return true;
		}

		public static bool ValidateString(string s, bool canBeEmpty)
		{
			// String is allowed to be empty or nil
			if (canBeEmpty && string.IsNullOrEmpty(s))
			{
				return true;
			}

			if (string.IsNullOrEmpty(s) || s.Length > 64)
			{
				return false;
			}
			return true;
		}

		public static bool ValidateLongString(string longString, bool canBeEmpty)
		{
			// String is allowed to be empty
			if (canBeEmpty && string.IsNullOrEmpty(longString))
			{
				return true;
			}

			if (string.IsNullOrEmpty(longString) || longString.Length > 8192)
			{
				return false;
			}
			return true;
		}

		public static bool ValidateConnectionType(string connectionType)
		{
			return StringMatch(connectionType, "^(wwan|wifi|lan|offline)$");
		}

		public static bool ValidateCustomDimensions(params string[] customDimensions)
		{
			return ValidateArrayOfStrings(20, 32, false, "custom dimensions", customDimensions);
		}

		public static bool ValidateResourceCurrencies(params string[] resourceCurrencies)
		{
			if (!ValidateArrayOfStrings(20, 64, false, "resource currencies", resourceCurrencies))
			{
				return false;
			}

			// validate each string for regex
			foreach (string resourceCurrency in resourceCurrencies)
			{
				if (!StringMatch(resourceCurrency, "^[A-Za-z]+$"))
				{
					Debug.Log("resource currencies validation failed: a resource currency can only be A-Z, a-z. String was: " + resourceCurrency);
					return false;
				}
			}
			return true;
		}

		public static bool ValidateResourceItemTypes(params string[] resourceItemTypes)
		{
			if (!ValidateArrayOfStrings(20, 32, false, "resource item types", resourceItemTypes))
			{
				return false;
			}

			// validate each resourceItemType for eventpart validation
			foreach (string resourceItemType in resourceItemTypes)
			{
				if (!ValidateEventPartCharacters(resourceItemType))
				{
					Debug.Log("resource item types validation failed: a resource item type cannot contain other characters than A-z, 0-9, -_. ()!?. String was: " + resourceItemType);
					return false;
				}
			}
			return true;
		}

		public static bool ValidateDimension01(string dimension01)
		{
			// we do not allow null
			if (string.IsNullOrEmpty(dimension01))
			{
				Debug.Log("Validation failed - custom dimension01 - value cannot be empty.");
				return false;
			}
			if (!GAState.HasAvailableCustomDimensions01(dimension01))
			{
				Debug.Log("Validation failed - custom dimension 01 - value was not found in list of custom dimensions 01 in the Settings object. \nGiven dimension value: " + dimension01);
				return false;
			}
			return true;
		}

		public static bool ValidateDimension02(string dimension02)
		{
			// we do not allow null
			if (string.IsNullOrEmpty(dimension02))
			{
				Debug.Log("Validation failed - custom dimension01 - value cannot be empty.");
				return false;
			}
			if (!GAState.HasAvailableCustomDimensions02(dimension02))
			{
				Debug.Log("Validation failed - custom dimension 02 - value was not found in list of custom dimensions 02 in the Settings object. \nGiven dimension value: " + dimension02);
				return false;
			}
			return true;
		}

		public static bool ValidateDimension03(string dimension03)
		{
			// we do not allow null
			if (string.IsNullOrEmpty(dimension03))
			{
				Debug.Log("Validation failed - custom dimension01 - value cannot be empty.");
				return false;
			}
			if (!GAState.HasAvailableCustomDimensions03(dimension03))
			{
				Debug.Log("Validation failed - custom dimension 03 - value was not found in list of custom dimensions 03 in the Settings object. \nGiven dimension value: " + dimension03);
				return false;
			}
			return true;
		}

		public static bool ValidateArrayOfStrings(long maxCount, long maxStringLength, bool allowNoValues, string logTag, params string[] arrayOfStrings)
		{
			string arrayTag = logTag;

			// use arrayTag to annotate warning log
			if (string.IsNullOrEmpty(arrayTag))
			{
				arrayTag = "Array";
			}

			if(arrayOfStrings == null)
			{
				Debug.Log(arrayTag + " validation failed: array cannot be null. ");
				return false;
			}

			// check if empty
			if (allowNoValues == false && arrayOfStrings.Length == 0)
			{
				Debug.Log(arrayTag + " validation failed: array cannot be empty. ");
				return false;
			}

			// check if exceeding max count
			if (maxCount > 0 && arrayOfStrings.Length > maxCount)
			{
				Debug.Log(arrayTag + " validation failed: array cannot exceed " + maxCount + " values. It has " + arrayOfStrings.Length + " values.");
				return false;
			}

			// validate each string
			foreach (string arrayString in arrayOfStrings)
			{
				int stringLength = arrayString == null ? 0 : arrayString.Length;
				// check if empty (not allowed)
				if (stringLength == 0)
				{
					Debug.Log(arrayTag + " validation failed: contained an empty string.");
					return false;
				}

				// check if exceeding max length
				if (maxStringLength > 0 && stringLength > maxStringLength)
				{
					Debug.Log(arrayTag + " validation failed: a string exceeded max allowed length (which is: " + maxStringLength + "). String was: " + arrayString);
					return false;
				}
			}
			return true;
		}

		public static bool ValidateClientTs(long clientTs)
		{
			// TODO(nikolaj): validate other way? (instead of max possible)
			if (clientTs < (long.MinValue+1) || clientTs > (long.MaxValue-1))
			{
				return false;
			}
			return true;
		}

		//public static bool ValidateCustomDimension01Settings(s

		#endregion // Public methods
	}
}
