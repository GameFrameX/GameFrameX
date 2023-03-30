using System;
using UnityEngine;

public class GAMopubIntegration
{
#if gameanalytics_mopub_enabled && !(UNITY_EDITOR)
    private static bool _subscribed = false;
#endif

    public static void ListenForImpressions(Action<string> callback)
    {
#if gameanalytics_mopub_enabled && !(UNITY_EDITOR)
        if (_subscribed)
        {
            Debug.Log("Ignoring duplicate gameanalytics subscription");
            return;
        }

        MoPubManager.OnImpressionTrackedEventBg += (arg1, arg2) => callback(arg2.JsonRepresentation);
        _subscribed = true;
#endif
    }
}
