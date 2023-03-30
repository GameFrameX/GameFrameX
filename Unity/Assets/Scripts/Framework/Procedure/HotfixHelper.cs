using System.Reflection;
using Framework.Asset;
using UnityEngine;

namespace UnityGameFramework.Procedure
{
    public static class HotfixHelper
    {
        public static void StartHotfix()
        {
            var assetPath = AssetUtility.GetCodePath("Hotfix.dll");
            var assetOperationHandle = GameApp.Asset.LoadAssetSync<Object>(assetPath);
            var assemblyData = assetOperationHandle.GetAssetObject<TextAsset>().bytes; // 从你的资源管理系统中获得热更新dll的数据
            var ass = Assembly.Load(assemblyData);
            var entryType = ass.GetType("Hotfix.HotfixLauncher");
            var method = entryType.GetMethod("Main");
            method?.Invoke(null, null);
        }
    }
}