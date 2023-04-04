using System.Reflection;
using Framework.Asset;

namespace UnityGameFramework.Procedure
{
    public static class HotfixHelper
    {
        public static async void StartHotfix()
        {
            var assetPath = AssetUtility.GetCodePath("Hotfix.dll");
            var assetOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetPath);
            var assemblyData = assetOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes; // 从你的资源管理系统中获得热更新dll的数据
            var ass = Assembly.Load(assemblyData);
            var entryType = ass.GetType("Hotfix.HotfixLauncher");
            var method = entryType.GetMethod("Main");
            method?.Invoke(null, null);
        }
    }
}