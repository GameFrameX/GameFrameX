using System.Reflection;
using Framework.Asset;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Procedure
{
    public static class HotfixHelper
    {
        public static async void StartHotfix()
        {
            var assetHotfixProtoDllPath = AssetUtility.GetCodePath("Unity.Hotfix.Proto.dll");
            var assetHotfixProtoDllOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixProtoDllPath);
            var assemblyDataHotfixProtoDll = assetHotfixProtoDllOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;

            var assetHotfixProtoPdbPath = AssetUtility.GetCodePath("Unity.Hotfix.Proto.pdb");
            var assetHotfixProtoPdbOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixProtoPdbPath);
            var assemblyDataHotfixProtoPdb = assetHotfixProtoPdbOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;

            Assembly.Load(assemblyDataHotfixProtoDll, assemblyDataHotfixProtoPdb);

            var assetHotfixDllPath = AssetUtility.GetCodePath("Unity.Hotfix.dll");
            var assetHotfixDllOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixDllPath);
            var assemblyDataHotfixDll = assetHotfixDllOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;

            var assetHotfixPdbPath = AssetUtility.GetCodePath("Unity.Hotfix.pdb");
            var assetHotfixPdbOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixPdbPath);
            var assemblyDataHotfixPdb = assetHotfixPdbOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;

            var ass = Assembly.Load(assemblyDataHotfixDll, assemblyDataHotfixPdb);
            var entryType = ass.GetType("Hotfix.HotfixLauncher");
            var method = entryType.GetMethod("Main");
            method?.Invoke(null, null);
        }
    }
}