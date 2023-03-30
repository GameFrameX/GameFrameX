using System;
using UnityEngine;

public class GATopOnIntegration
{
#if gameanalytics_topon_enabled && !(UNITY_EDITOR)
    private static bool _subscribed = false;
    private class GAInterstitialListener : AnyThinkAds.Api.ATInterstitialAdListener
    {
        private Action<string> callback;

        public GAInterstitialListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void onInterstitialAdClick(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onInterstitialAdClose(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onInterstitialAdEndPlayingVideo(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onInterstitialAdFailedToPlayVideo(string placementId, string code, string message) {}

        public void onInterstitialAdFailedToShow(string placementId) {}

        public void onInterstitialAdLoad(string placementId) {}

        public void onInterstitialAdLoadFail(string placementId, string code, string message) {}

        public void onInterstitialAdShow(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo)
        {
            callback(callbackInfo.getOriginJSONString());
        }

        public void onInterstitialAdStartPlayingVideo(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}
    }
    private class GABannerListener : AnyThinkAds.Api.ATBannerAdListener
    {
        private Action<string> callback;

        public GABannerListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void onAdAutoRefresh(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onAdAutoRefreshFail(string placementId, string code, string message) {}

        public void onAdClick(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onAdClose(string placementId) {}

        public void onAdCloseButtonTapped(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onAdImpress(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo)
        {
            callback(callbackInfo.getOriginJSONString());
        }

        public void onAdLoad(string placementId) {}

        public void onAdLoadFail(string placementId, string code, string message) {}
    }
    private class GARewardedListener : AnyThinkAds.Api.ATRewardedVideoListener
    {
        private Action<string> callback;

        public GARewardedListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void onRewardedVideoAdLoaded(string placementId) {}

        public void onRewardedVideoAdLoadFail(string placementId, string code, string message) {}

        public void onRewardedVideoAdPlayStart(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo)
        {
            callback(callbackInfo.getOriginJSONString());
        }

        public void onRewardedVideoAdPlayEnd(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onRewardedVideoAdPlayFail(string placementId, string code, string message) {}

        public void onRewardedVideoAdPlayClosed(string placementId, bool isReward, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onRewardedVideoAdPlayClicked(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onReward(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}
    }

    private class GANativeListener : AnyThinkAds.Api.ATNativeAdListener
    {
        private Action<string> callback;

        public GANativeListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void onAdClicked(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onAdCloseButtonClicked(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo) {}

        public void onAdImpressed(string placementId, AnyThinkAds.Api.ATCallbackInfo callbackInfo)
        {
            callback(callbackInfo.getOriginJSONString());
        }

        public void onAdLoaded(string placementId) {}

        public void onAdLoadFail(string placementId, string code, string message) {}

        public void onAdVideoEnd(string placementId) {}

        public void onAdVideoProgress(string placementId, int progress) {}

        public void onAdVideoStart(string placementId) {}
    }
#endif

    public static void ListenForImpressions(Action<string> callback)
    {
#if gameanalytics_topon_enabled && !(UNITY_EDITOR)
        if (_subscribed)
        {
            Debug.Log("Ignoring duplicate gameanalytics subscription");
            return;
        }

        AnyThinkAds.Api.ATInterstitialAd.Instance.setListener(new GAInterstitialListener(callback));
        AnyThinkAds.Api.ATBannerAd.Instance.setListener(new GABannerListener(callback));
        AnyThinkAds.Api.ATRewardedVideo.Instance.setListener(new GARewardedListener(callback));
        AnyThinkAds.Api.ATNativeAd.Instance.setListener(new GANativeListener(callback));
        _subscribed = true;
#endif
    }
}
