using System;
using GameFrameX.Runtime;
using Cysharp.Threading.Tasks;
using GameFrameX.Startup.Runtime;
#if ENABLE_GAME_FRAME_X_HYBRID_CLR && !DISABLE_HYBRIDCLR
using System.Reflection;
using System.Linq;
using HybridCLR;
#endif

namespace GameFrameX.Startup.Application
{
    /// <summary>
    /// 热更启动辅助类，负责加载 AOT DLL 与热更程序集，并调用其入口方法。
    /// </summary>
    /// <remarks>
    /// Hotfix startup helper responsible for loading AOT DLLs and the hotfix assembly, then invoking its entry method.
    /// </remarks>
    public static class HotfixHelper
    {
        /// <summary>
        /// 以默认选项启动热更程序集。
        /// </summary>
        /// <remarks>
        /// Start the hotfix assembly with default options.
        /// </remarks>
        public static async void StartHotfix()
        {
            await StartHotfixAsync(null);
        }

        /// <summary>
        /// 异步加载并启动热更程序集。
        /// </summary>
        /// <remarks>
        /// Asynchronously load and start the hotfix assembly.
        /// </remarks>
        /// <param name="options">启动选项；为 null 时使用默认配置 / Startup options; default configuration is used when null</param>
        /// <returns>包含启动结果与错误信息的 <see cref="HotfixLaunchResult"/> / The <see cref="HotfixLaunchResult"/> containing startup result and error info</returns>
        public static async UniTask<HotfixLaunchResult> StartHotfixAsync(StartupOptions options)
        {
            try
            {
                await StartHotfixInternalAsync(options);
                return HotfixLaunchResult.Succeed();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return HotfixLaunchResult.Fail(e.Message);
            }
        }

        private static
#if ENABLE_GAME_FRAME_X_HYBRID_CLR && !DISABLE_HYBRIDCLR
            async UniTask
#else
            UniTask
#endif
            StartHotfixInternalAsync(StartupOptions options)
        {
            var assemblyName = options == null || string.IsNullOrEmpty(options.HotfixAssemblyName)
                ? "Unity.Hotfix"
                : options.HotfixAssemblyName;
            var entryTypeName = options == null || string.IsNullOrEmpty(options.HotfixEntryTypeName)
                ? "Hotfix.HotfixLauncher"
                : options.HotfixEntryTypeName;
            var entryMethodName = options == null || string.IsNullOrEmpty(options.HotfixEntryMethodName)
                ? "Main"
                : options.HotfixEntryMethodName;

#if ENABLE_GAME_FRAME_X_HYBRID_CLR && !DISABLE_HYBRIDCLR
            if (ApplicationHelper.IsEditor)
            {
                var assemblies = Utility.Assembly.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                    {
                        Run(assembly, entryTypeName, entryMethodName);
                        break;
                    }
                }

                return;
            }

            Log.Info("开始加载AOT DLL");
            var aotDlls = AOTGenericReferences.PatchedAOTAssemblyList.ToArray();
            foreach (var aotDll in aotDlls)
            {
                Log.Info("开始加载AOT DLL ==> " + aotDll);
                var assetHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(Utility.Asset.Path.GetAOTCodePath(aotDll));
                var aotBytes = assetHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;
                RuntimeApi.LoadMetadataForAOTAssembly(aotBytes, HomologousImageMode.SuperSet);
            }

            Log.Info("结束加载AOT DLL");
            Log.Info("开始加载" + assemblyName + ".dll");
            var assetHotfixDllPath = Utility.Asset.Path.GetCodePath(assemblyName + Utility.Const.FileNameSuffix.DLL);
            var assetHotfixDllOperationHandle = await GameApp.Asset.LoadAssetAsync<UnityEngine.Object>(assetHotfixDllPath);
            var assemblyDataHotfixDll = assetHotfixDllOperationHandle.GetAssetObject<UnityEngine.TextAsset>().bytes;
            Log.Info("开始加载程序集Hotfix");
            var hotfixAssembly = Assembly.Load(assemblyDataHotfixDll, null);
            Run(hotfixAssembly, entryTypeName, entryMethodName);

#else
            RunNative(entryTypeName, entryMethodName);
            return UniTask.CompletedTask;
#endif
        }
#if ENABLE_GAME_FRAME_X_HYBRID_CLR && !DISABLE_HYBRIDCLR
        private static void Run(Assembly assembly, string entryTypeName, string entryMethodName)
        {
            Log.Info("加载程序集Hotfix 结束 Assembly " + assembly.FullName);
            var entryType = assembly.GetType(entryTypeName);
            Log.Info("加载程序集Hotfix 结束 EntryType " + entryType.FullName);
            var method = entryType.GetMethod(entryMethodName);
            Log.Info("加载程序集Hotfix 结束 EntryType=>method " + method?.Name);
            method?.Invoke(null, null);
        }
#else
        private static void RunNative(string entryTypeName, string entryMethodName)
        {
            var entryType = Utility.Assembly.GetType(entryTypeName);
            Log.Info("加载程序集Hotfix 结束 EntryType " + entryType.FullName);
            var method = entryType.GetMethod(entryMethodName);
            Log.Info("加载程序集Hotfix 结束 EntryType=>method " + method?.Name);
            method?.Invoke(null, null);
        }
#endif
    }
}
