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
        /// 获取工程路径
        /// </summary>
        /// <returns></returns>
        private static string GetProjectPath()
        {
            return Application.dataPath.Replace("Assets", string.Empty);
        }

        /// <summary>
        /// 发布 APK
        /// </summary>
        [MenuItem("Tools/Build/Apk", false, 20)]
        private static void BuildAssetBundleForAndroid()
        {
            PlayerSettings.SplashScreen.show = false;

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

            string apkPath =
                $"{GetProjectPath()}/Apks/{Application.version}/{Application.identifier}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_v_{PlayerSettings.bundleVersion}_code_{PlayerSettings.Android.bundleVersionCode}.apk";
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, apkPath, BuildTarget.Android,
                BuildOptions.None);
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

        static string GeneratorGradleByWrapper(string path)
        {
            File.WriteAllText(path + "/build_init.bat", "gradle wrapper");

            return path + "/build_init.bat";
        }

        static string GeneratorGradlewByAssembleDebug(string path)
        {
            File.WriteAllText(path + "/build_debug.bat", "gradlew assembleDebug");

            return path + "/build_debug.bat";
        }

        static string GeneratorOutPutPathByDebug(string path)
        {
            string outputPath = GetOutPutPath(path, "debug");

            File.WriteAllText(path + "/build_output_debug.bat",
                $"start {outputPath}");

            return path + "/build_output_debug.bat";
        }

        static string GeneratorOutPutPathByRelease(string path)
        {
            string outputPath = GetOutPutPath(path, "release");

            File.WriteAllText(path + "/build_output_release.bat",
                $"start {outputPath}");

            return path + "/build_output_release.bat";
        }

        static string GeneratorGradlewByAssembleRelease(string path)
        {
            string outputPath = GetOutPutPath(path, "release");

            File.WriteAllText(path + "/build_release.bat",
                $"gradlew assembleRelease");

            return path + "/build_release.bat";
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
        /// 发布 AS Release 版本
        /// </summary>
        [MenuItem("Tools/Build/AS Project Release", false, 20)]
        private static void ExportToAndroidStudioToRelease()
        {
            PlayerSettings.SplashScreen.show = false;
            var apkPath = GetBuildPath();

            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, apkPath, BuildTarget.Android,
                BuildOptions.None);
            Debug.Log(apkPath);
            GeneratorGradleByWrapper(apkPath);
            GeneratorGradlewByAssembleRelease(apkPath);
            GeneratorOutPutPathByRelease(apkPath);
            Debug.LogError("发布目录:" + apkPath);
        }

        /// <summary>
        /// 获取发布路径
        /// </summary>
        /// <returns></returns>
        private static string GetBuildPath()
        {
            DirectoryInfo dataDir = new DirectoryInfo(Application.dataPath);

            string workPathName = "Build";
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneOSX:
                case BuildTarget.iOS:
                    workPathName = "Xcode";

                    break;
                case BuildTarget.Android:
                    workPathName = "ASWork";

                    break;
            }

            string apkPath =
                $"{dataDir.Parent.Parent.FullName}/{workPathName}/{Application.version}/{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
            DirectoryInfo asWorkDir = new DirectoryInfo(apkPath);
            if (asWorkDir.Exists)
            {
                asWorkDir.Delete(true);
            }
            else
            {
                asWorkDir.Create();
            }

            return apkPath;
        }

        public static Process ProcessRun(string exe, string arguments, string workingDirectory = ".",
            bool waitExit = false)
        {
            try
            {
                bool redirectStandardOutput = true;
                bool redirectStandardError = true;
                bool useShellExecute = false;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    redirectStandardOutput = false;
                    redirectStandardError = false;
                    useShellExecute = true;
                }

                if (waitExit)
                {
                    redirectStandardOutput = true;
                    redirectStandardError = true;
                    useShellExecute = false;
                }

                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = useShellExecute,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = redirectStandardOutput,
                    RedirectStandardError = redirectStandardError,
                };

                Process process = Process.Start(info);

                if (waitExit)
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new Exception(
                            $"{process.StandardOutput.ReadToEnd()} {process.StandardError.ReadToEnd()}");
                    }
                }

                return process;
            }
            catch (Exception e)
            {
                throw new Exception($"dir: {Path.GetFullPath(workingDirectory)}, command: {exe} {arguments}", e);
            }
        }

        /// <summary>
        /// 发布 AS Debug 版本
        /// </summary>
        [MenuItem("Tools/Build/AS Project Debug", false, 20)]
        private static void ExportToAndroidStudioToDevelop()
        {
            PlayerSettings.SplashScreen.show = false;
            string apkPath = GetBuildPath();

            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            EditorUserBuildSettings.development = true;

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, apkPath, BuildTarget.Android,
                BuildOptions.None);
            Debug.Log(apkPath);

            GeneratorGradleByWrapper(apkPath);
            GeneratorGradlewByAssembleDebug(apkPath);
            GeneratorOutPutPathByDebug(apkPath);
            Process.Start(apkPath);
        }

        /// <summary>
        /// 发布 Xcode Debug 版本
        /// </summary>
        [MenuItem("Tools/Build/Xcode Project Debug", false, 30)]
        private static void ExportToXcodeToDevelop()
        {
            PlayerSettings.SplashScreen.show = false;
            string xcodePath = GetBuildPath();

            EditorUserBuildSettings.development = true;

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, xcodePath, BuildTarget.iOS,
                BuildOptions.None);
            Process.Start(xcodePath);
            Debug.LogError("发布目录:" + xcodePath);
        }

        /// <summary>
        /// 发布 Xcode Release 版本
        /// </summary>
        [MenuItem("Tools/Build/Xcode Project Release", false, 30)]
        private static void ExportToXcodeToRelease()
        {
            PlayerSettings.SplashScreen.show = false;
            string xcodePath = GetBuildPath();

            EditorUserBuildSettings.development = false;

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, xcodePath, BuildTarget.iOS,
                BuildOptions.None);
            Process.Start(xcodePath);
            Debug.LogError("发布目录:" + xcodePath);
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
                PlayerSettings.Android.bundleVersionCode =
                    Convert.ToInt32(PlayerSettings.Android.bundleVersionCode) + 1;
            }

            if (target == BuildTarget.iOS)
            {
                // Update Build Version Code
                PlayerSettings.iOS.buildNumber =
                    (Convert.ToInt32(PlayerSettings.iOS.buildNumber) + 1).ToString();
            }
        }
    }
}