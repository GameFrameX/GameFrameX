using System;
using UnityEngine;
using System.Collections.Generic;
using GameAnalyticsSDK.Utilities;

public class GAMaxIntegration
{
#if gameanalytics_max_enabled && !(UNITY_EDITOR)
    private static bool _subscribed = false;

    [Serializable]
    public class MaxImpressionData
    {
        public string country;
        public string network_name;
        public string adunit_id;
        public string adunit_format;
        public string placement;
        public string creative_id;
        public float revenue;
    }

    private static void runCallback(string format, MaxSdkBase.AdInfo adInfo, Action<string> callback)
    {
        MaxImpressionData data = new MaxImpressionData();
        data.country = MaxSdk.GetSdkConfiguration().CountryCode;
        data.network_name = adInfo.NetworkName;
        data.adunit_id =  adInfo.AdUnitIdentifier;
        data.adunit_format = format;
        data.placement = adInfo.Placement;
        data.creative_id = adInfo.CreativeIdentifier;
        data.revenue = (float)adInfo.Revenue;
        string json = JsonUtility.ToJson(data);
        callback(json);
    }
#endif

    public static void ListenForImpressions(Action<string> callback)
    {
#if gameanalytics_max_enabled && !(UNITY_EDITOR)
        if (_subscribed)
        {
            Debug.Log("Ignoring duplicate gameanalytics subscription");
            return;
        }

        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (adUnitId, adInfo) => runCallback("INTER", adInfo, callback);
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (adUnitId, adInfo) => runCallback("BANNER", adInfo, callback);
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (adUnitId, adInfo) => runCallback("REWARDED", adInfo, callback);
        MaxSdkCallbacks.CrossPromo.OnAdRevenuePaidEvent += (adUnitId, adInfo) => runCallback("XPROMO", adInfo, callback);
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += (adUnitId, adInfo) => runCallback("MREC", adInfo, callback);
        MaxSdkCallbacks.RewardedInterstitial.OnAdRevenuePaidEvent += (adUnitId, adInfo) => runCallback("REWARDED_INTER", adInfo, callback);
        _subscribed = true;
#endif
    }
}
