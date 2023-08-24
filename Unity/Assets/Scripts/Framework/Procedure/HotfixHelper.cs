using System.Reflection;
using Framework.Asset;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Procedure
{
    public static class HotfixHelper
    {
        public static async void StartHotfix()
        {
            var assetHotfixProtoPath = AssetUtility.GetCodePath("Unity.Hotfix.Proto.dll");
            var assetHotfixProtoOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixProtoPath);
            var assemblyDataHotfixProto = assetHotfixProtoOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;
            Assembly.Load(assemblyDataHotfixProto);

            var assetHotfixPath = AssetUtility.GetCodePath("Unity.Hotfix.dll");
            var assetHotfixOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixPath);
            var assemblyDataHotfix = assetHotfixOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;
            var ass = Assembly.Load(assemblyDataHotfix);
            var entryType = ass.GetType("Hotfix.HotfixLauncher");
            var method = entryType.GetMethod("Main");
            method?.Invoke(null, null);
        }
    }
}