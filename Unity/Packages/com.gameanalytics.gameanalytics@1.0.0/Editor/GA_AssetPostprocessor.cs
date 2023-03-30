using UnityEditor;

namespace GameAnalyticsSDK.Editor
{
    public class GA_AssetPostprocessor : AssetPostprocessor
    {
        private const string AssetsPrependPath = GA_SettingsInspector.IsCustomPackage ? "Packages/com.gameanalytics.sdk/Runtime" : "Assets/GameAnalytics";
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            #region iOS and tvOS
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/tvOS/GameAnalyticsTVOS.h") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.tvOS) || importer.GetCompatibleWithPlatform(BuildTarget.iOS)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/tvOS/GameAnalyticsTVOSUnity.m") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.tvOS) || importer.GetCompatibleWithPlatform(BuildTarget.iOS)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/tvOS/libGameAnalyticsTVOS.a") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.tvOS) || importer.GetCompatibleWithPlatform(BuildTarget.iOS)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            #endregion // iOS and tvOS
            #region General
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/GameAnalytics.dll") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() ||
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneLinux64) ||
#if UNITY_2019_2_OR_NEWER
#else
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneLinux) ||
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal) ||
#endif
#if UNITY_2017_3_OR_NEWER
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneOSX) ||
#else
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel) ||
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64) ||
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal) ||
#endif
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows) ||
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64) ||
                    importer.GetCompatibleWithPlatform(BuildTarget.WSAPlayer)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, true);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, true);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, true);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.Tizen, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            #endregion // General
            #region Standalone
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/Windows/x86/sqlite3.dll") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/Windows/x64/sqlite3.dll") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/Linux/sqlite3.so") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() ||
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneLinux64)
#if UNITY_2019_2_OR_NEWER
#else
                    || !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneLinux) ||
                    !importer.GetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal)
#endif
                ))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, true);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, true);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            #endregion // Standalone
            #region WebGL
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/WebGL/GameAnalytics.WebGL.dll") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.WebGL)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/WebGL/HandleIO.jslib") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.WebGL)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/WebGL/Mono.Data.Sqlite.dll") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.WebGL)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/WebGL/sqlite.c") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.WebGL)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/WebGL/sqlite.h") as PluginImporter;
                if(importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.WebGL)))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, true);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, false);
                    importer.SaveAndReimport();
                }
            }
            #endregion // WebGL
            #region WSA
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/WSA/x86/GameAnalytics.UWP.dll") as PluginImporter;
                if (importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.WSAPlayer) ||
                    !importer.GetPlatformData(BuildTarget.WSAPlayer, "SDK").Equals("UWP") ||
                    !importer.GetPlatformData(BuildTarget.WSAPlayer, "CPU").Equals("X86") ||
                    !importer.GetPlatformData(BuildTarget.WSAPlayer, "ScriptingBackend").Equals("Il2Cpp")))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, true);
                    importer.SetPlatformData(BuildTarget.WSAPlayer, "SDK", "UWP");
                    importer.SetPlatformData(BuildTarget.WSAPlayer, "CPU", "X86");
                    importer.SetPlatformData(BuildTarget.WSAPlayer, "ScriptingBackend", "Il2Cpp");
                    importer.SaveAndReimport();
                }
            }
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/WSA/x64/GameAnalytics.UWP.dll") as PluginImporter;
                if (importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.WSAPlayer) ||
                    !importer.GetPlatformData(BuildTarget.WSAPlayer, "SDK").Equals("UWP") ||
                    !importer.GetPlatformData(BuildTarget.WSAPlayer, "CPU").Equals("X64") ||
                    !importer.GetPlatformData(BuildTarget.WSAPlayer, "ScriptingBackend").Equals("Il2Cpp")))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, true);
                    importer.SetPlatformData(BuildTarget.WSAPlayer, "SDK", "UWP");
                    importer.SetPlatformData(BuildTarget.WSAPlayer, "CPU", "X64");
                    importer.SetPlatformData(BuildTarget.WSAPlayer, "ScriptingBackend", "Il2Cpp");
                    importer.SaveAndReimport();
                }
            }
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetsPrependPath + "/Plugins/WSA/ARM/GameAnalytics.UWP.dll") as PluginImporter;
                if (importer != null && (importer.GetCompatibleWithAnyPlatform() || !importer.GetCompatibleWithPlatform(BuildTarget.WSAPlayer) ||
                    !importer.GetPlatformData(BuildTarget.WSAPlayer, "SDK").Equals("UWP") ||
                    !importer.GetPlatformData(BuildTarget.WSAPlayer, "CPU").Equals("ARM") ||
                    !importer.GetPlatformData(BuildTarget.WSAPlayer, "ScriptingBackend").Equals("Il2Cpp")))
                {
                    importer.SetCompatibleWithAnyPlatform(false);
                    importer.SetCompatibleWithEditor(false);
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
#if UNITY_2019_2_OR_NEWER
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);
#endif
#if UNITY_2017_3_OR_NEWER
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);
#else
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXUniversal, false);
#endif
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.iOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.tvOS, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
                    importer.SetCompatibleWithPlatform(BuildTarget.WSAPlayer, true);
                    importer.SetPlatformData(BuildTarget.WSAPlayer, "SDK", "UWP");
                    importer.SetPlatformData(BuildTarget.WSAPlayer, "CPU", "ARM");
                    importer.SetPlatformData(BuildTarget.WSAPlayer, "ScriptingBackend", "Il2Cpp");
                    importer.SaveAndReimport();
                }
            }
            #endregion // WSA
        }
    }
}
