using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;


public static class HttpHelper
{
    public static Dictionary<string, string> GetBaseParams()
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary["Language"] = Application.systemLanguage.ToString();
        dictionary["PackageName"] = Application.identifier;
        dictionary["AppVersion"] = Application.version;
        dictionary["Platform"] = PathHelper.GetPlatformName;
        dictionary["Channel"] = BlankGetChannel.GetChannelName();
        dictionary["SubChannel"] = BlankGetChannel.GetChannelName();

        // if (GlobalConfigComponent.Instance.GlobalProto.DevelopmentMode)
        // {
        //     dictionary["Channel"] = "develop";
        //     dictionary["SubChannel"] = "develop";
        // }

        return dictionary;
    }
}