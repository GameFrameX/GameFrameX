using System;
using UnityEngine;

public class GAHyperBidIntegration : MonoBehaviour
{
#if gameanalytics_hyperbid_enabled && !(UNITY_EDITOR)
    private static bool _subscribed = false;
#endif

    public static void ListenForImpressions(Action<string> callback)
    {
#if gameanalytics_hyperbid_enabled && !(UNITY_EDITOR)
        if (_subscribed)
        {
            Debug.Log("Ignoring duplicate gameanalytics subscription");
            return;
        }

        HyperBid.Api.HBInterstitialAd.Instance.events.onAdShowEvent += (sender, args) => callback(args.callbackInfo.getOriginJSONString());
        HyperBid.Api.HBBannerAd.Instance.events.onAdImpressEvent += (sender, args) => callback(args.callbackInfo.getOriginJSONString());
        HyperBid.Api.HBRewardedVideo.Instance.events.onAdVideoStartEvent += (sender, args) => callback(args.callbackInfo.getOriginJSONString());
        HyperBid.Api.HBNativeAd.Instance.events.onAdImpressEvent += (sender, args) => callback(args.callbackInfo.getOriginJSONString());
        _subscribed = true;
#endif
    }
}
