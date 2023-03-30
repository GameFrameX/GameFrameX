using UnityEditor.Callbacks;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GameAnalyticsSDK.Editor
{
#if UNITY_2018_1_OR_NEWER
    public class GA_PostprocessBuild : UnityEditor.Build.IPreprocessBuildWithReport
#else
    public class GA_PostprocessBuild
#endif
    {
        private static string gameanalytics_mopub = "gameanalytics_mopub_enabled";
        private static string gameanalytics_fyber = "gameanalytics_fyber_enabled";
        private static string gameanalytics_ironsource = "gameanalytics_ironsource_enabled";
        private static string gameanalytics_topon = "gameanalytics_topon_enabled";
        private static string gameanalytics_max = "gameanalytics_max_enabled";
        private static string gameanalytics_aequus = "gameanalytics_aequus_enabled";
        private static string gameanalytics_hyperbid = "gameanalytics_hyperbid_enabled";
        private static string gameanalytics_admob = "gameanalytics_admob_enabled";

#if UNITY_2018_1_OR_NEWER
        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            Update3rdPartyIntegrations();
        }
#endif

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            Update3rdPartyIntegrations();
        }

        private static void Update3rdPartyIntegrations()
        {
            UpdateMoPub();
            UpdateFyber();
            UpdateIronSource();
            UpdateTopOn();
            UpdateMax();
            UpdateAequus();
            UpdateHyperBid();
            UpdateAdMob();
        }

        private static void UpdateDefines(string entry, bool enabled, BuildTargetGroup[] groups)
        {
            foreach (var group in groups)
            {
                var defines = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                var edited = false;
                if (enabled && !defines.Contains(entry))
                {
                    defines.Add(entry);
                    edited = true;
                }
                else if (!enabled && defines.Contains(entry))
                {
                    defines.Remove(entry);
                    edited = true;
                }
                if (edited) {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
                }
            }
        }

#region 3rd Party Lib Detection

        /// <summary>
        /// Sets the scripting define symbol `gameanalytics_mopub_enabled` to true if MoPub classes are detected within the Unity project
        /// </summary>
        private static void UpdateMoPub()
        {
            var mopubTypes = new string[] { "MoPubBase", "MoPubManager" };
            if (TypeExists(mopubTypes))
            {
                UpdateDefines(gameanalytics_mopub, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
            else
            {
                UpdateDefines(gameanalytics_mopub, false, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
        }

        /// <summary>
        /// Sets the scripting define symbol `gameanalytics_fyber_enabled` to true if Fyber classes are detected within the Unity project
        /// </summary>
        private static void UpdateFyber()
        {
            var fyberTypes = new string[] { "Fyber.Interstitial", "Fyber.Banner", "Fyber.Rewarded" };
            if (TypeExists(fyberTypes))
            {
                UpdateDefines(gameanalytics_fyber, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
            else
            {
                UpdateDefines(gameanalytics_fyber, false, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
        }

        /// <summary>
        /// Sets the scripting define symbol `gameanalytics_ironsource_enabled` to true if IronSource classes are detected within the Unity project
        /// </summary>
        private static void UpdateIronSource()
        {
            var ironSourceTypes = new string[] { "IronSourceEvents", "IronSource" };
            if (TypeExists(ironSourceTypes))
            {
                UpdateDefines(gameanalytics_ironsource, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
            else
            {
                UpdateDefines(gameanalytics_ironsource, false, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
        }

        /// <summary>
        /// Sets the scripting define symbol `gameanalytics_topon_enabled` to true if TopOn classes are detected within the Unity project
        /// </summary>
        private static void UpdateTopOn()
        {
            var topOnTypes = new string[] { "AnyThinkAds.Api.ATBannerAd", "AnyThinkAds.Api.ATInterstitialAd", "AnyThinkAds.Api.ATRewardedVideo", "AnyThinkAds.Api.ATNativeAd" };
            if (TypeExists(topOnTypes))
            {
                UpdateDefines(gameanalytics_topon, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
            else
            {
                UpdateDefines(gameanalytics_topon, false, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
        }

        /// <summary>
        /// Sets the scripting define symbol `gameanalytics_max_enabled` to true if Max classes are detected within the Unity project
        /// </summary>
        private static void UpdateMax()
        {
            var maxTypes = new string[] { "MaxSdkCallbacks", "MaxSdk", "MaxSdkBase.AdInfo" };
            if (TypeExists(maxTypes))
            {
                UpdateDefines(gameanalytics_max, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
            else
            {
                UpdateDefines(gameanalytics_max, false, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
        }

        /// <summary>
        /// Sets the scripting define symbol `gameanalytics_aequus_enabled` to true if Aequus classes are detected within the Unity project
        /// </summary>
        private static void UpdateAequus()
        {
            var aequusTypes = new string[] { "Mobi.Aequus.Sdk.Aequus", "Mobi.Aequus.Sdk.AequusILRDListener", "Mobi.Aequus.Sdk.ImpressionData" };
            if (TypeExists(aequusTypes))
            {
                UpdateDefines(gameanalytics_aequus, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
            else
            {
                UpdateDefines(gameanalytics_aequus, false, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
        }

        /// <summary>
        /// Sets the scripting define symbol `gameanalytics_hyperbid_enabled` to true if HyperBid classes are detected within the Unity project
        /// </summary>
        private static void UpdateHyperBid()
        {
            var topOnTypes = new string[] { "HyperBid.Api.HBBannerAd", "HyperBid.Api.HBInterstitialAd", "HyperBid.Api.HBRewardedVideo", "HyperBid.Api.HBNativeAd" };
            if (TypeExists(topOnTypes))
            {
                UpdateDefines(gameanalytics_hyperbid, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
            else
            {
                UpdateDefines(gameanalytics_hyperbid, false, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
        }

        /// <summary>
        /// Sets the scripting define symbol `gameanalytics_admob_enabled` to true if AdMob classes are detected within the Unity project
        /// </summary>
        private static void UpdateAdMob()
        {
            var topOnTypes = new string[] { "GoogleMobileAds.Api.AdRequest", "GoogleMobileAds.Api.BannerView", "GoogleMobileAds.Api.InterstitialAd", "GoogleMobileAds.Api.RewardedAd", "GoogleMobileAds.Api.RewardedInterstitialAd" };
            if (TypeExists(topOnTypes))
            {
                UpdateDefines(gameanalytics_admob, true, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
            else
            {
                UpdateDefines(gameanalytics_admob, false, new BuildTargetGroup[] { BuildTargetGroup.iOS, BuildTargetGroup.Android });
            }
        }

        private static bool TypeExists(params string[] types)
        {
            if (types == null || types.Length == 0)
                return false;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (types.Any(type => assembly.GetType(type) != null))
                    return true;
            }

            return false;
        }

#endregion


        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS || buildTarget == BuildTarget.tvOS)
            {
#if UNITY_IOS || UNITY_TVOS
                string projPath = UnityEditor.iOS.Xcode.PBXProject.GetPBXProjectPath(path);

                UnityEditor.iOS.Xcode.PBXProject proj = new UnityEditor.iOS.Xcode.PBXProject();
                proj.ReadFromString(File.ReadAllText(projPath));

#if UNITY_2019_3_OR_NEWER
                string target = proj.GetUnityMainTargetGuid();
#else
                string targetName = UnityEditor.iOS.Xcode.PBXProject.GetUnityTargetName();
                string target = proj.TargetGuidByName(targetName);
#endif

                proj.AddFileToBuild(target, proj.AddFile("usr/lib/libsqlite3.dylib", "Frameworks/libsqlite3.dylib", UnityEditor.iOS.Xcode.PBXSourceTree.Sdk));
                proj.AddFileToBuild(target, proj.AddFile("usr/lib/libz.dylib", "Frameworks/libz.dylib", UnityEditor.iOS.Xcode.PBXSourceTree.Sdk));
                proj.AddFrameworkToProject(target, "AdSupport.framework", false);
                proj.AddFrameworkToProject(target, "AppTrackingTransparency.framework", true);
                //proj.SetBuildProperty(target, "ENABLE_BITCODE", "YES");
#if gameanalytics_topon_enabled
                string toponSubPath = "Libraries/GameAnalytics/Plugins/iOS/GameAnalyticsTopOnHelper.m";
                string[] topOnGuids = AssetDatabase.FindAssets("GameAnalyticsTopOnHelper", null);
                if (topOnGuids.Length > 0)
                {
                    string[] p = AssetDatabase.GUIDToAssetPath(topOnGuids[0]).Split(new char[] { '/' }, 2);
                    if(p.Length > 1)
                    {
                        toponSubPath = "Libraries/" + p[1];
                    }
                }
                string toponHelperFilePath = Path.Combine(path, toponSubPath);
                string topOncontents = File.ReadAllText(toponHelperFilePath);
                topOncontents = topOncontents.Replace("#if gameanalytics_topon_enabled", "").Replace("#endif", "");
                File.WriteAllText(toponHelperFilePath, topOncontents);
#endif
#if gameanalytics_hyperbid_enabled
                string hyperbidSubPath = "Libraries/GameAnalytics/Plugins/iOS/GameAnalyticsHyperBidHelper.m";
                string[] hyperBidGuids = AssetDatabase.FindAssets("GameAnalyticsHyperBidHelper", null);
                if (hyperBidGuids.Length > 0)
                {
                    string[] p = AssetDatabase.GUIDToAssetPath(hyperBidGuids[0]).Split(new char[] { '/' }, 2);
                    if(p.Length > 1)
                    {
                        hyperbidSubPath = "Libraries/" + p[1];
                    }
                }
                string hyperbidHelperFilePath = Path.Combine(path, hyperbidSubPath);
                string hyperbidContents = File.ReadAllText(hyperbidHelperFilePath);
                hyperbidContents = hyperbidContents.Replace("#if gameanalytics_hyperbid_enabled", "").Replace("#endif", "");
                File.WriteAllText(hyperbidHelperFilePath, hyperbidContents);
#endif

                File.WriteAllText(projPath, proj.WriteToString());
#endif
            }

        }
    }
}
