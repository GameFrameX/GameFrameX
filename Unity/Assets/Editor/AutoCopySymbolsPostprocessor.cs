using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System;

public class AutoCopySymbolsPostprocessor
{
    [PostProcessBuild()]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.Android)
            PostProcessAndroidBuild(pathToBuiltProject);
    }

    public static void PostProcessAndroidBuild(string pathToBuiltProject)
    {
        CopyAndroidSymbols(pathToBuiltProject, PlayerSettings.Android.targetArchitectures);
    }

    public static void CopyAndroidSymbols(string pathToBuiltProject, AndroidArchitecture targetDevice)
    {
        var startTime = DateTime.Now;
        Debug.Log(nameof(CopyAndroidSymbols) + " Start ");
        string buildName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
        string symbolsDir = buildName + "_Symbols/";
        var abi_v7a = "armeabi-v7a/";
        var abi_v8a = "arm64-v8a/";

        CreateDir(symbolsDir);

        if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) > 0)
        {
            var dir = CopySymbols(symbolsDir, abi_v8a);
            GenerateAllSymbols(dir);
        }

        if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARMv7) > 0)
        {
            var dir = CopySymbols(symbolsDir, abi_v7a);
            GenerateAllSymbols(dir);
        }

#if UNITY_2018_1_OR_NEWER

#else
        if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.X86) > 0)
        {
            var dir = CopySymbols(symbolsDir, abi_x86);
            GenerateAllSymbols(dir);
        }
#endif
        Debug.Log(nameof(CopyAndroidSymbols) + " End ==>" + (DateTime.Now - startTime).TotalSeconds);
    }

    const string LibPath = "/../Temp/StagingArea/libs/";

    private static string CopySymbols(string symbolsDir, string abi)
    {
        //const string abi = "arm64-v8a/";
        string sourceDir = Application.dataPath + LibPath + abi;
        string abiDir = symbolsDir + abi;
        CreateDir(abiDir);
        MoveAllFiles(sourceDir, abiDir);
        // "".Print("CopySymbols", symbolsDir, abi);
        return abiDir;
    }

    public static void CreateDir(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);

        Directory.CreateDirectory(path);
    }

    public static void MoveAllFiles(string src, string dst)
    {
        DirectoryInfo srcinfo = new DirectoryInfo(src);
        if (srcinfo.Exists)
        {
            dst = dst.Replace("\\", "/");
            FileInfo[] files = srcinfo.GetFiles("*.*");
            for (int i = 0; i < files.Length; i++)
            {
                if (File.Exists(dst + "/" + files[i].Name))
                {
                    File.Delete(dst + "/" + files[i].Name);
                }

                File.Copy(files[i].FullName, dst + "/" + files[i].Name);
            }
        }
    }

    public static void GenerateAllSymbols(string symbolsdir)
    {
        DirectoryInfo srcinfo = new DirectoryInfo(symbolsdir);
        if (srcinfo.Exists)
        {
            string cmd = Application.dataPath;
            string soPath = Application.dataPath;
            string jdk = EditorPrefs.GetString("JdkPath");
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                // cmd += "/../BuglySymbol/buglySymbolAndroid.jar";
                cmd += "/../BuglySymbol/buglyqq-upload-symbol.jar";
                jdk += "/bin/java";
                soPath += "/../" + symbolsdir;
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // cmd += "/../BuglySymbol/buglySymbolAndroid.jar";
                cmd += "/../BuglySymbol/buglyqq-upload-symbol.jar";
                cmd = cmd.Replace("/", "\\");
                jdk = jdk.Replace("/", "\\") + "\\bin\\java.exe";
                soPath += "/../" + symbolsdir;
                soPath = soPath.Replace("/", "\\");
            }

            // var appID = "ef9301f8fb";
            // var appKey = "ea81cbea-41b4-49d0-a40a-0dcad0cf4c6b";
            // var channelTag = GameConfig.Instance().TryGetString("CHANNEL_TAG", "com.bc.avenger");
            // var buglyChannelAsset = Resources.Load<com.bugly.sdk.BuglyChannelAsset>("bugly_config");
            // var buglyCfg = buglyChannelAsset.GetBuglyChannelCfg(channelTag);
            // if (buglyCfg == null)
            // {
            //     Debug.LogError("can not find bugly channel data:" + channelTag);
            // }
            //
            // bool q1DebugValue = GameConfig.Instance().TryGetBool("ENABLE_Q1_DEBUG_MODE", false);
            // if (buglyCfg != null)
            // {
            //     if (q1DebugValue)
            //     {
            //         appID = buglyCfg.debugAppID;
            //         appKey = buglyCfg.debugAppKey;
            //     }
            //     else
            //     {
            //         appID = buglyCfg.appID;
            //         appKey = buglyCfg.appKey;
            //     }
            // }
            //
            // Debug.Log("bugly symbolsdir:" + symbolsdir);
//
// #if UPLOAD_SYMBOLS || true
//             // ProcessCommand(jdk, "-jar " + cmd + " -i " + symbolsdir + " -u -id " + appID + " - key " + appKey + " - package " + PlayerSettings.applicationIdentifier + " -version " + PlayerSettings.bundleVersion);
//             string commandStr = "-jar " + cmd + " -appid " + appID + " -appkey " + appKey + " -bundleid  " + PlayerSettings.applicationIdentifier + " -version " + PlayerSettings.bundleVersion + " -platform Android" + " -inputSymbol " + soPath;
//
//             Debug.Log("bugly ProcessCommand str:" + commandStr);
//             ProcessCommand(jdk, commandStr);
// #else
//             ProcessCommand(jdk, "-jar " + cmd + " -i " + symbolsdir);
// #endif
            //ProcessCommand(cmd,"-i " + symbolsdir + " -u -id 844a29e21e -key b85577b4-1347-40bb-a880-f8a91446007f -package " + PlayerSettings.applicationIdentifier + " -version " + PlayerSettings.bundleVersion);
        }
    }

    public static void ProcessCommand(string command, string argument)
    {
        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(command);
        info.Arguments = argument;
        info.CreateNoWindow = false;
        info.ErrorDialog = true;
        info.UseShellExecute = true;

        if (info.UseShellExecute)
        {
            info.RedirectStandardOutput = false;
            info.RedirectStandardError = false;
            info.RedirectStandardInput = false;
        }
        else
        {
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.RedirectStandardInput = true;
            info.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            info.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
        }

        System.Diagnostics.Process process = System.Diagnostics.Process.Start(info);

        if (!info.UseShellExecute)
        {
            Debug.Log(process.StandardOutput);
            Debug.Log(process.StandardError);
        }

        process.WaitForExit();
        process.Close();
    }
}