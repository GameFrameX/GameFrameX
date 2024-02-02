using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GameFrameX;
using UnityEditor;
using UnityEngine;

namespace Unity.Editor
{
    [InitializeOnLoad]
    public class BuildHotfixEditor
    {
        //Unity代码生成dll位置
        private const string HotFixAssembliesDir = "Library/ScriptAssemblies";
        private static readonly string ScriptAssembliesDir = $"HybridCLRData/HotUpdateDlls/{EditorUserBuildSettings.activeBuildTarget}";


        private static readonly string[] HotfixDlls = new string[] { "Unity.Hotfix.dll", "Unity.Hotfix.pdb", "Unity.Hotfix.Proto.dll", "Unity.Hotfix.Proto.pdb" };


        //热更代码存放位置
        private const string CodeDir = "Assets/Bundles/Code/";

        static BuildHotfixEditor()
        {
            Func<Task> waitExcute = async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                //拷贝热更代码
                CopyCode();
            };
            waitExcute();
        }

        /// <summary>
        /// 复制代码
        /// </summary>
        [MenuItem("Tools/Build/Copy Hotfix Code")]
        static void CopyCode()
        {
            if (!Directory.Exists(CodeDir))
            {
                Directory.CreateDirectory(CodeDir);
            }

            foreach (var hotfix in HotfixDlls)
            {
                // string srcPath = Path.Combine(ScriptAssembliesDir, hotfix);
                // if (!File.Exists(srcPath))
                // {
                var srcPath = Path.Combine(HotFixAssembliesDir, hotfix);
                // }

                File.Copy(srcPath, Path.Combine(CodeDir, hotfix + Utility.Const.FileNameSuffix.Binary), true);
            }

            Debug.Log($"复制Hotfix DLL, Hotfix pdb到{CodeDir}完成");
            AssetDatabase.Refresh();
        }

        private const string AOTCodeDir = "Assets/Bundles/AOTCode/";

        /// <summary>
        /// 复制AOT代码
        /// </summary>
        [MenuItem("Tools/Build/Copy AOT Code")]
        static void CopyAOTCode()
        {
            if (!Directory.Exists(AOTCodeDir))
            {
                Directory.CreateDirectory(AOTCodeDir);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath);
            string path = Path.Combine(directoryInfo.Parent.FullName, "HybridCLRData", "AssembliesPostIl2CppStrip", EditorUserBuildSettings.activeBuildTarget.ToString());
            Debug.Log(path);

            DirectoryInfo aotCodeDir = new DirectoryInfo(path);
            var files = aotCodeDir.GetFiles("*.dll");
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var fileInfo in files)
            {
                stringBuilder.AppendLine(fileInfo.Name);
                fileInfo.CopyTo(AOTCodeDir + "/" + fileInfo.Name + Utility.Const.FileNameSuffix.Binary, true);
            }

            Debug.Log(stringBuilder);
            Debug.Log($"复制AOT DLL, Hotfix pdb到{CodeDir}完成");
            AssetDatabase.Refresh();
        }
    }
}