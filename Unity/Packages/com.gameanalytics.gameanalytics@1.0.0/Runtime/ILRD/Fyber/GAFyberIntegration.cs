using System;
using UnityEngine;

public class GAFyberIntegration
{
#if gameanalytics_fyber_enabled && !(UNITY_EDITOR)
    private static bool _subscribed = false;
    private class GAInterstitialListener : Fyber.InterstitialListener
    {
        private Action<string> callback;

        public GAInterstitialListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void OnShow(string placementId, Fyber.ImpressionData impressionData)
        {
            callback(impressionData.ToString());
        }

        public void OnClick(string placementId) {}

        public void OnHide(string placementId) {}

        public void OnShowFailure(string placementId, Fyber.ImpressionData impressionData) {}

        public void OnAvailable(string placementId) {}

        public void OnUnavailable(string placementId) {}

        public void OnRequestStart(string placementId) {}
    }
    private class GABannerListener : Fyber.BannerListener
    {
        private Action<string> callback;

        public GABannerListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void OnError(string placementId, string error) {}

        public void OnLoad(string placementId) {}

        public void OnShow(string placementId, Fyber.ImpressionData impressionData)
        {
            callback(impressionData.ToString());
        }

        public void OnClick(string placementId) {}

        public void OnRequestStart(string placementId) {}
    }
    private class GARewardedListener : Fyber.RewardedListener
    {
        private Action<string> callback;

        public GARewardedListener(Action<string> callback)
        {
            this.callback = callback;
        }

        public void OnShow(string placementId, Fyber.ImpressionData impressionData)
        {
            callback(impressionData.ToString());
        }

        public void OnClick(string placementId) {}

        public void OnHide(string placementId) {}

        public void OnShowFailure(string placementId, Fyber.ImpressionData impressionData) {}

        public void OnAvailable(string placementId) {}

        public void OnUnavailable(string placementId) {}

        public void OnCompletion(string placementId, bool userRewarded) {}

        public void OnRequestStart(string placementId) {}
    }
#endif

    public static void ListenForImpressions(Action<string> callback)
    {
#if gameanalytics_fyber_enabled && !(UNITY_EDITOR)
        if (_subscribed)
        {
            Debug.Log("Ignoring duplicate gameanalytics subscription");
            return;
        }

        Fyber.Interstitial.SetInterstitialListener(new GAInterstitialListener(callback));
        Fyber.Banner.SetBannerListener(new GABannerListener(callback));
        Fyber.Rewarded.SetRewardedListener(new GARewardedListener(callback));
        _subscribed = true;
#endif
    }
}
