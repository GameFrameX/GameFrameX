using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.Editor
{
    /// <summary>
    ///  导出和发布产品
    /// </summary>
    public static class BuildProductEditor
    {
        /// <summary>
        /// 发布 WebGL
        /// </summary>
        [MenuItem("Tools/Build/WebGL", false, 20)]
        private static void BuildPlayerToWebGL()
        {
            PlayerSettings.SplashScreen.show = false;

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                Debug.LogError("当前构建目标不是 WebGL");
                return;
            }

            UpdateBuildTime();
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, BuildOutputPath(), BuildTarget.WebGL, BuildOptions.None);
            Debug.LogError("发布目录:" + BuildOutputPath());
        }

        /// <summary>
        /// 发布 APK
        /// </summary>
        [MenuItem("Tools/Build/Apk", false, 20)]
        private static void BuildPlayerToAndroid()
        {
            PlayerSettings.SplashScreen.show = false;
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.LogError("当前构建目标不是 Android");
                return;
            }

            UpdateBuildTime();
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

            string apkPath = $"{BuildOutputPath()}.apk";
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, apkPath, BuildTarget.Android, BuildOptions.None);
            Debug.LogError("发布目录:" + apkPath);
        }

        /// <summary>
        /// 发布 AAB
        /// </summary>
        [MenuItem("Tools/Build/AAB", false, 20)]
        private static void BuildAppBundleForAndroid()
        {
            PlayerSettings.SplashScreen.show = false;

            string aabSavePath = Application.dataPath.Replace("Assets", "AAB");
            if (!Directory.Exists(aabSavePath))
            {
                Directory.CreateDirectory(aabSavePath);
            }

            string aabFileName = Application.version + "-" + PlayerSettings.Android.bundleVersionCode + ".aab";

            // var aabFilePath = EditorUtility.SaveFilePanel("Create Android App Bundle", null, null, "aab");
            var aabFilePath =
                aabSavePath + "/" +
                aabFileName; // EditorUtility.SaveFilePanel("Create Android App Bundle", null, null, "aab");
            if (string.IsNullOrEmpty(aabFilePath))
            {
                Debug.LogError("输出路径异常,取消打包AAB");
                return;
            }


            if (string.IsNullOrEmpty(PlayerSettings.Android.keystoreName)
                || string.IsNullOrEmpty(PlayerSettings.Android.keyaliasName)
                || string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass)
                || string.IsNullOrEmpty(PlayerSettings.Android.keystorePass))
            {
                Debug.LogError("没有设置签名密钥,取消打包AAB");
                return;
            }

            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

            EditorUserBuildSettings.buildAppBundle = true;
            // 开启符号表的输出
            EditorUserBuildSettings.androidCreateSymbolsZip = true;

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, aabFilePath, BuildTarget.Android, BuildOptions.None);

            Debug.Log("AAB存储路径=>" + aabFilePath);
        }

        /// <summary>
        /// 复制build.Gradle 到目标目录
        /// </summary>
        /// <param name="targetPath"></param>
        private static void CopyFileByBuildGradle(string targetPath)
        {
#if UNITY_ANDROID
            var resourcesPath = Application.dataPath + "buildConfig/build.gradle";
            if (File.Exists(resourcesPath))
            {
                var path = targetPath + "/build.gradle";
                File.Copy(resourcesPath, path, true);
            }
#endif
        }

        static string GeneratorGradleByWrapper(string path, string extensionSuffix = ".bat")
        {
            File.WriteAllText(path + "/build_init" + extensionSuffix, "gradle wrapper");

            return path + "/build_init" + extensionSuffix;
        }

        static string GeneratorGradlewByAssembleDebug(string path)
        {
            File.WriteAllText(path + "/build_debug.bat", "gradlew assembleDebug");
            return path + "/build_debug.bat";
        }

        static string GeneratorOutPutPathByDebug(string path, string extensionSuffix = ".bat")
        {
            string outputPath = GetOutPutPath(path, "debug");

            File.WriteAllText(path + "/build_output_debug" + extensionSuffix, $"start {outputPath}");

            return path + "/build_output_debug" + extensionSuffix;
        }

        static void GeneratorGradle(string outputPath)
        {
#if UNITY_EDITOR_WIN
            string extensionSuffix = ".bat";
#else
            string extensionSuffix = ".sh";
#endif
            GeneratorGradleByWrapper(outputPath, extensionSuffix);
            GeneratorGradlewByAssembleRelease(outputPath, extensionSuffix);
            GeneratorOutPutPathByRelease(outputPath, extensionSuffix);
        }

        static string GeneratorOutPutPathByRelease(string path, string extensionSuffix = ".bat")
        {
            string outputPath = GetOutPutPath(path, "release");

            File.WriteAllText(path + "/build_output_release" + extensionSuffix, $"start {outputPath}");

            return path + "/build_output_release.bat";
        }

        static string GeneratorGradlewByAssembleRelease(string path, string extensionSuffix = ".bat")
        {
            string outputPath = GetOutPutPath(path, "release");

            File.WriteAllText(path + "/build_release" + extensionSuffix, $"gradlew assembleRelease");

            return path + "/build_release" + extensionSuffix;
        }

        /// <summary>
        /// 获取程序的输出目录
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static string GetOutPutPath(string path, string name)
        {
            return $"{path}/launcher/build/outputs/apk/{name}/";
        }

        /// <summary>
        /// 发布 AS Debug 版本
        /// </summary>
        [MenuItem("Tools/Build/AS Project Debug", false, 20)]
        private static void ExportToAndroidStudioToDevelop()
        {
            PlayerSettings.SplashScreen.show = false;
            UpdateBuildTime();
            string buildOutputPath = BuildOutputPath();

            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            EditorUserBuildSettings.development = true;

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildOutputPath, BuildTarget.Android, BuildOptions.None);
            Debug.Log(buildOutputPath);

            GeneratorGradle(buildOutputPath);
            CopyFileByBuildGradle(buildOutputPath);
            Process.Start(buildOutputPath);
        }

        /// <summary>
        /// 发布 AS Release 版本
        /// </summary>
        [MenuItem("Tools/Build/AS Project Release", false, 20)]
        private static void ExportToAndroidStudioToRelease()
        {
            PlayerSettings.SplashScreen.show = false;
            UpdateBuildTime();
            var buildOutputPath = BuildOutputPath();

            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildOutputPath, BuildTarget.Android, BuildOptions.None);
            Debug.Log(buildOutputPath);
            GeneratorGradle(buildOutputPath);
            CopyFileByBuildGradle(buildOutputPath);
            Debug.LogError("发布目录:" + buildOutputPath);
        }


        /// <summary>
        /// 发布 Xcode Debug 版本
        /// </summary>
        [MenuItem("Tools/Build/Xcode Project Debug", false, 30)]
        private static void ExportToXcodeToDevelop()
        {
            PlayerSettings.SplashScreen.show = false;
            UpdateBuildTime();
            string buildOutputPath = BuildOutputPath();

            EditorUserBuildSettings.development = true;

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildOutputPath, BuildTarget.iOS, BuildOptions.None);
            Process.Start(buildOutputPath);
            Debug.LogError("发布目录:" + buildOutputPath);
        }

        /// <summary>
        /// 发布 Xcode Release 版本
        /// </summary>
        [MenuItem("Tools/Build/Xcode Project Release", false, 30)]
        private static void ExportToXcodeToRelease()
        {
            PlayerSettings.SplashScreen.show = false;
            UpdateBuildTime();
            string buildOutputPath = BuildOutputPath();

            EditorUserBuildSettings.development = false;

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildOutputPath, BuildTarget.iOS, BuildOptions.None);
            Process.Start(buildOutputPath);
            Debug.LogError("发布目录:" + buildOutputPath);
        }

        /// <summary>
        /// 设置 发布版本号更新
        /// </summary>
        /// <param name="target"></param>
        /// <param name="path"></param>
        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.Android)
            {
                // Update Build Version Code
                PlayerSettings.Android.bundleVersionCode = Convert.ToInt32(PlayerSettings.Android.bundleVersionCode) + 1;
            }

            if (target == BuildTarget.iOS)
            {
                // Update Build Version Code
                PlayerSettings.iOS.buildNumber = (Convert.ToInt32(PlayerSettings.iOS.buildNumber) + 1).ToString();
            }
        }

        /// <summary>
        /// 获取工程路径
        /// </summary>
        /// <returns></returns>
        private static string GetProjectPath()
        {
            return Application.dataPath.Replace("Assets", string.Empty);
        }

        /// <summary>
        /// 构建导出根目录
        /// </summary>
        private static string GetBuildRootPath
        {
            get { return $"{GetProjectPath()}/Builds"; }
        }

        private static string _buildTime;

        static BuildProductEditor()
        {
            _buildTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        }

        /// <summary>
        /// 更新时间命名
        /// </summary>
        private static void UpdateBuildTime()
        {
            _buildTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        }

        /// <summary>
        /// 获取发布导出路径
        /// </summary>
        /// <returns></returns>
        private static string BuildOutputPath()
        {
            string pathName = $"{Application.identifier}_{_buildTime}_v_{PlayerSettings.bundleVersion}_code_{PlayerSettings.Android.bundleVersionCode}";
            string path = Path.Combine(GetBuildRootPath, EditorUserBuildSettings.activeBuildTarget.ToString(), Application.version, Application.identifier, pathName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}