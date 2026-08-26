using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Editor
{
    internal static class ProjectSettingEditor
    {
        [InitializeOnLoadMethod]
        static void Start()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.blank.gfx");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.blank.gfx");
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.productName = "GFX";
            PlayerSettings.companyName = "ALianBlank";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            // 禁用其他应用的声音
            PlayerSettings.muteOtherAudioSources = false;
            // 隐藏状态条
            PlayerSettings.statusBarHidden = true;
            PlayerSettings.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            // 取消多线程渲染
            PlayerSettings.MTRendering = false;
            // 取消骨骼加速.目前是负优化
            PlayerSettings.gpuSkinning = false;
            PlayerSettings.assemblyVersionValidation = false;
#if UNITY_ANDROID
            PlayerSettings.Android.renderOutsideSafeArea = true;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
            if (EditorUserBuildSettings.development)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Debug);
            }
            else
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
            }

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.androidTVCompatibility = false;
            // PlayerSettings.Android.chromeosInputEmulation = false;
            // PlayerSettings.bundleVersion = "1.0.0";
#endif
#if UNITY_IOS
            PlayerSettings.iOS.appleDeveloperTeamID = "XXXXXX";
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            PlayerSettings.iOS.hideHomeButton = true;
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1);
#endif

            var folderList = new string[] { "Assets/StreamingAssets", "Assets/Bundles/AOTCode", "Assets/Bundles/Code", "Assets/Bundles/Shader", "Assets/Bundles/Textures", "Assets/Bundles/Sprites", "Assets/Bundles/Config", "Assets/Bundles/Sound", "Assets/Bundles/UI" };
            foreach (var folder in folderList)
            {
                // 本地文件夹是否存在()
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            // AssetDatabase.SaveAssets();
        }
    }
}