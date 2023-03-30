using System;
using UnityEngine;
using System.Collections.Generic;
using GameAnalyticsSDK.Utilities;

public class GAAequusIntegration
{
#if gameanalytics_aequus_enabled && !(UNITY_EDITOR)
    private static bool _subscribed = false;

    private class GAAequusListener : Mobi.Aequus.Sdk.AequusILRDListener
    {
        private Action<string, string> callback;

        public GAAequusListener(Action<string, string> callback)
        {
            this.callback = callback;
        }

        public void OnAequusILRDImpressionLoad(Mobi.Aequus.Sdk.ImpressionData impressionData)
        {
        }

        public void OnAequusILRDImpressionShow(Mobi.Aequus.Sdk.ImpressionData impressionData)
        {
            callback(impressionData.AequusSdkVersion, impressionData.GetJsonRepresentation());
        }
    }
#endif

    public static void ListenForImpressions(Action<string, string> callback)
    {
#if gameanalytics_aequus_enabled && !(UNITY_EDITOR)
        if (_subscribed)
        {
            Debug.Log("Ignoring duplicate gameanalytics subscription");
            return;
        }

        Mobi.Aequus.Sdk.Aequus.SetILRDListener(new GAAequusListener(callback));
        _subscribed = true;
#endif
    }
}
