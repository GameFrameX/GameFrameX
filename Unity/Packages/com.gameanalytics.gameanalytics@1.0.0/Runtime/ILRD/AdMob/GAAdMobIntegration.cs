using System;
using UnityEngine;
using System.Collections.Generic;
using GameAnalyticsSDK.Utilities;

public class GAAdMobIntegration
{
#if gameanalytics_admob_enabled && !(UNITY_EDITOR)

    [Serializable]
    public class AdMobImpressionData
    {
        public string adunit_id;
        public string currency;
        public int precision;
        public string adunit_format;
        public string network_class_name;
        public long revenue;
    }

    public static void ListenForImpressions(string adUnitId, GoogleMobileAds.Api.BannerView ad, Action<string, string> callback)
    {
        ad.OnPaidEvent += (sender, args) => {
            AdMobImpressionData data = new AdMobImpressionData();
            data.adunit_id = adUnitId;
            data.currency = args.AdValue.CurrencyCode;
            data.precision = (int)args.AdValue.Precision;
            data.adunit_format = "BANNER";
            data.network_class_name = ad.GetResponseInfo().GetMediationAdapterClassName();
            data.revenue = args.AdValue.Value;
            string json = JsonUtility.ToJson(data);
            callback(GoogleMobileAds.Api.AdRequest.Version, json);
        };
    }

    public static void ListenForImpressions(string adUnitId, GoogleMobileAds.Api.InterstitialAd ad, Action<string, string> callback)
    {
        ad.OnPaidEvent += (sender, args) => {
            AdMobImpressionData data = new AdMobImpressionData();
            data.adunit_id = adUnitId;
            data.currency = args.AdValue.CurrencyCode;
            data.precision = (int)args.AdValue.Precision;
            data.adunit_format = "INTERSTITIAL";
            data.network_class_name = ad.GetResponseInfo().GetMediationAdapterClassName();
            data.revenue = args.AdValue.Value;
            string json = JsonUtility.ToJson(data);
            callback(GoogleMobileAds.Api.AdRequest.Version, json);
        };
    }

    public static void ListenForImpressions(string adUnitId, GoogleMobileAds.Api.RewardedAd ad, Action<string, string> callback)
    {
        ad.OnPaidEvent += (sender, args) => {
            AdMobImpressionData data = new AdMobImpressionData();
            data.adunit_id = adUnitId;
            data.currency = args.AdValue.CurrencyCode;
            data.precision = (int)args.AdValue.Precision;
            data.adunit_format = "REWARDED_VIDEO";
            data.network_class_name = ad.GetResponseInfo().GetMediationAdapterClassName();
            data.revenue = args.AdValue.Value;
            string json = JsonUtility.ToJson(data);
            callback(GoogleMobileAds.Api.AdRequest.Version, json);
        };
    }

    public static void ListenForImpressions(string adUnitId, GoogleMobileAds.Api.RewardedInterstitialAd ad, Action<string, string> callback)
    {
        ad.OnPaidEvent += (sender, args) => {
            AdMobImpressionData data = new AdMobImpressionData();
            data.adunit_id = adUnitId;
            data.currency = args.AdValue.CurrencyCode;
            data.precision = (int)args.AdValue.Precision;
            data.adunit_format = "REWARDED_INTERSTITIAL";
            data.network_class_name = ad.GetResponseInfo().GetMediationAdapterClassName();
            data.revenue = args.AdValue.Value;
            string json = JsonUtility.ToJson(data);
            callback(GoogleMobileAds.Api.AdRequest.Version, json);
        };
    }
#endif
}
