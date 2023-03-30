using System;
using UnityEngine;

public class GAIronSourceIntegration
{
#if gameanalytics_ironsource_enabled && !(UNITY_EDITOR)
    private static bool _subscribed = false;
#endif

    public static void ListenForImpressions(Action<string> callback)
    {
#if gameanalytics_ironsource_enabled && !(UNITY_EDITOR)
        if (_subscribed)
        {
            Debug.Log("Ignoring duplicate gameanalytics subscription");
            return;
        }

        IronSourceEvents.onImpressionSuccessEvent += (arg1) => callback(arg1.allData);
        _subscribed = true;
#endif

    }
}
