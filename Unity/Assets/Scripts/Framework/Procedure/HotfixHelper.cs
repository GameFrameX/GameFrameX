using System.Reflection;
using Framework.Asset;
using GameFrameX.Runtime;
using HybridCLR;
using UnityEngine;

namespace GameFrameX.Procedure
{
    public static class HotfixHelper
    {
        private static string[] aotDlls = new string[]
        {
            "GameFrameX.Runtime.dll",
            "LuBan.Runtime.dll",
            "MessagePack.Runtime.dll",
            "System.Core.dll",
            "System.Memory.dll",
            "System.Numerics.Vectors.dll",
            "System.Runtime.CompilerServices.Unsafe.dll",
            "UniTask.Runtime.dll",
            "mscorlib.dll",

            "Assembly-CSharp.dll",
            "BestHTTP.dll",
            "BetterStreamingAssets.dll",
            "BlankGetChannel.Runtime.dll",
            "BlankReadAssets.Runtime.dll",
            "DOTween.dll",
            "FairyGUI.Runtime.dll",
            "GameFrameX.Runtime.dll",
            "HybridCLR.Runtime.dll",
            "ICSharpCode.SharpZipLib.dll",
            "io.sentry.unity.runtime.dll",
            "LuBan.Runtime.dll",
            "MessagePack.Annotations.dll",
            "MessagePack.Runtime.dll",
            "Mono.Security.dll",
            "mscorlib.dll",
            "netstandard.dll",
            "Newtonsoft.Json.dll",
            "Sentry.dll",
            "Sentry.Microsoft.Bcl.AsyncInterfaces.dll",
            "Sentry.System.Buffers.dll",
            "Sentry.System.Collections.Immutable.dll",
            "Sentry.System.Memory.dll",
            "Sentry.System.Numerics.Vectors.dll",
            "Sentry.System.Reflection.Metadata.dll",
            "Sentry.System.Runtime.CompilerServices.Unsafe.dll",
            "Sentry.System.Text.Encodings.Web.dll",
            "Sentry.System.Text.Json.dll",
            "Sentry.System.Threading.Tasks.Extensions.dll",
            "Sentry.Unity.dll",
            "SimpleJSON.Runtime.dll",
            "System.Buffers.dll",
            "System.Configuration.dll",
            "System.Core.dll",
            "System.Data.dll",
            "System.Diagnostics.StackTrace.dll",
            "System.dll",
            "System.Globalization.Extensions.dll",
            "System.IO.Compression.dll",
            "System.IO.Pipelines.dll",
            "System.Memory.dll",
            "System.Net.Http.dll",
            "System.Numerics.dll",
            "System.Numerics.Vectors.dll",
            "System.Runtime.CompilerServices.Unsafe.dll",
            "System.Runtime.Serialization.dll",
            "System.Threading.Tasks.Extensions.dll",
            "System.Xml.dll",
            "System.Xml.Linq.dll",
            "UniTask.Runtime.dll",
            "Unity.Startup.dll",
            "UnityEngine.AIModule.dll",
            "UnityEngine.AndroidJNIModule.dll",
            "UnityEngine.AnimationModule.dll",
            "UnityEngine.AssetBundleModule.dll",
            "UnityEngine.AudioModule.dll",
            "UnityEngine.CoreModule.dll",
            "UnityEngine.DirectorModule.dll",
            "UnityEngine.dll",
            "UnityEngine.GridModule.dll",
            "UnityEngine.ImageConversionModule.dll",
            "UnityEngine.IMGUIModule.dll",
            "UnityEngine.InputLegacyModule.dll",
            "UnityEngine.InputModule.dll",
            "UnityEngine.JSONSerializeModule.dll",
            "UnityEngine.ParticleSystemModule.dll",
            "UnityEngine.Physics2DModule.dll",
            "UnityEngine.PhysicsModule.dll",
            "UnityEngine.ScreenCaptureModule.dll",
            "UnityEngine.SharedInternalsModule.dll",
            "UnityEngine.SubsystemsModule.dll",
            "UnityEngine.TerrainModule.dll",
            "UnityEngine.TextRenderingModule.dll",
            "UnityEngine.TilemapModule.dll",
            "UnityEngine.UI.dll",
            "UnityEngine.UIElementsModule.dll",
            "UnityEngine.UIModule.dll",
            "UnityEngine.UnityAnalyticsModule.dll",
            "UnityEngine.UnityWebRequestAssetBundleModule.dll",
            "UnityEngine.UnityWebRequestModule.dll",
            "UnityEngine.VFXModule.dll",
            "UnityEngine.VideoModule.dll",
            "UnityEngine.VRModule.dll",
            "UnityEngine.XRModule.dll",
            "UnityWebSocket.Runtime.dll",
            "YooAsset.Runtime.dll",
        };

        public static async void StartHotfix()
        {
            Log.Info("开始加载AOT DLL");
            foreach (var aotDll in aotDlls)
            {
                // Log.Info("开始加载AOT DLL ==> " + aotDll);
                var assetHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(AssetUtility.GetAOTCodePath(aotDll));
                var aotBytes = assetHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;
                RuntimeApi.LoadMetadataForAOTAssembly(aotBytes, HomologousImageMode.SuperSet);
            }

            Log.Info("结束加载AOT DLL");
            // RuntimeApi.LoadMetadataForAOTAssembly()

            Log.Info("开始加载Unity.Hotfix.Proto.dll");
            var assetHotfixProtoDllPath = AssetUtility.GetCodePath("Unity.Hotfix.Proto.dll");
            var assetHotfixProtoDllOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixProtoDllPath);
            var assemblyDataHotfixProtoDll = assetHotfixProtoDllOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;
            Log.Info("开始加载Unity.Hotfix.Proto.pdb");
            var assetHotfixProtoPdbPath = AssetUtility.GetCodePath("Unity.Hotfix.Proto.pdb");
            var assetHotfixProtoPdbOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixProtoPdbPath);
            var assemblyDataHotfixProtoPdb = assetHotfixProtoPdbOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;
            Log.Info("开始加载程序集Proto");
            Assembly.Load(assemblyDataHotfixProtoDll, assemblyDataHotfixProtoPdb);
            Log.Info("开始加载Unity.Hotfix.dll");
            var assetHotfixDllPath = AssetUtility.GetCodePath("Unity.Hotfix.dll");
            var assetHotfixDllOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixDllPath);
            var assemblyDataHotfixDll = assetHotfixDllOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;
            Log.Info("开始加载Unity.Hotfix.pdb");
            var assetHotfixPdbPath = AssetUtility.GetCodePath("Unity.Hotfix.pdb");
            var assetHotfixPdbOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixPdbPath);
            var assemblyDataHotfixPdb = assetHotfixPdbOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;
            Log.Info("开始加载程序集Hotfix");
            var ass = Assembly.Load(assemblyDataHotfixDll, assemblyDataHotfixPdb);

            Log.Info("加载程序集Hotfix 结束 Assembly " + ass.FullName);
            var entryType = ass.GetType("Hotfix.HotfixLauncher");
            Log.Info("加载程序集Hotfix 结束 EntryType " + entryType.FullName);
            var method = entryType.GetMethod("Main");
            Log.Info("加载程序集Hotfix 结束 EntryType=>method " + method?.Name);
            method?.Invoke(null, null);
        }
    }
}