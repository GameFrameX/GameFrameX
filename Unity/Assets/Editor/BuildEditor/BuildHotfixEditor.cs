using System;
using System.IO;
using System.Threading.Tasks;
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

        //热更代码dll文件
        private const string HotfixDll = "Unity.Hotfix.dll";

        //热更代码pdb文件
        private const string HotfixPdb = "Unity.Hotfix.pdb";

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
        [MenuItem("Tools/Build/CopyCode")]
        static void CopyCode()
        {
            if (!Directory.Exists(CodeDir))
            {
                Directory.CreateDirectory(CodeDir);
            }

            if (Directory.Exists(ScriptAssembliesDir))
            {
                File.Copy(Path.Combine(ScriptAssembliesDir, HotfixDll), Path.Combine(CodeDir, "Hotfix.dll.bytes"), true);
                File.Copy(Path.Combine(ScriptAssembliesDir, HotfixPdb), Path.Combine(CodeDir, "Hotfix.pdb.bytes"), true);
            }
            else
            {
                File.Copy(Path.Combine(HotFixAssembliesDir, HotfixDll), Path.Combine(CodeDir, "Hotfix.dll.bytes"), true);
                File.Copy(Path.Combine(HotFixAssembliesDir, HotfixPdb), Path.Combine(CodeDir, "Hotfix.pdb.bytes"), true);
            }

            Debug.Log($"复制Hotfix.dll, Hotfix.pdb到{CodeDir}完成");
            AssetDatabase.Refresh();
        }
    }
}