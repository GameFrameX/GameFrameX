using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Asset")]
    public sealed class AssetComponent : GameFrameworkComponent
    {
        public EPlayMode GamePlayMode;
        private AssetsPackage buildinPackage;
        public const string BuildInPackageName = "DefaultPackage";


        private InitializationOperation initializationOperation;
        private string _hostServer;
        public string StaticVersion { get; private set; }

        protected override void Awake()
        {
            base.Awake();
        }

        public void UpdateStaticVersion(string version)
        {
            StaticVersion = version;
        }

        public async UniTask Initialize(string host)
        {
            _hostServer = host;
            Debug.Log($"资源系统运行模式：{GamePlayMode}");
            YooAssets.Initialize();
            YooAssets.SetOperationSystemMaxTimeSlice(30);
            YooAssets.SetCacheSystemCachedFileVerifyLevel(EVerifyLevel.High);
            YooAssets.SetDownloadSystemBreakpointResumeFileSize(4096 * 8);

            // 创建默认的资源包
            buildinPackage = YooAssets.TryGetAssetsPackage(BuildInPackageName);
            if (buildinPackage == null)
            {
                buildinPackage = YooAssets.CreateAssetsPackage(BuildInPackageName);
                // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
                YooAssets.SetDefaultAssetsPackage(buildinPackage);
            }

            if (GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                // 编辑器下的模拟模式
                InitializeYooAssetEditorSimulateMode();
            }
            else if (GamePlayMode == EPlayMode.OfflinePlayMode)
            {
                // 单机运行模式
                InitializeYooAssetOfflinePlayMode();
            }
            else if (GamePlayMode == EPlayMode.HostPlayMode)
            {
                // 联机运行模式
                InitializeYooAssetHostPlayMode();
            }

            await initializationOperation.ToUniTask();

            Debug.Log("Asset Init Over");
        }

        private void InitializeYooAssetEditorSimulateMode()
        {
            var initParameters = new EditorSimulateModeParameters();
            initParameters.SimulatePatchManifestPath = EditorSimulateModeHelper.SimulateBuild(BuildInPackageName);
            initParameters.AssetLoadingMaxNumber = 10;
            initializationOperation = buildinPackage.InitializeAsync(initParameters);
        }

        private void InitializeYooAssetOfflinePlayMode()
        {
            var initParameters = new OfflinePlayModeParameters();
            // initParameters.DecryptionServices = new GameDecryptionServices();
            initializationOperation = buildinPackage.InitializeAsync(initParameters);
        }

        private void InitializeYooAssetHostPlayMode()
        {
            var initParameters = new HostPlayModeParameters();
            initParameters.QueryServices = new QueryStreamingAssetsFileServices();
            initParameters.DefaultHostServer = _hostServer;
            initParameters.FallbackHostServer = _hostServer;
            initializationOperation = buildinPackage.InitializeAsync(initParameters);
        }

        /// <summary>
        /// 内置文件查询服务类
        /// </summary>
        private class QueryStreamingAssetsFileServices : IQueryServices
        {
            public bool QueryStreamingAssets(string fileName)
            {
                // 注意：使用了BetterStreamingAssets插件，使用前需要初始化该插件！
                string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
                return BetterStreamingAssets.FileExists($"{buildinFolderName}/{fileName}");
            }
        }

        #region 异步加载子资源对象

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public UniTask<SubAssetsOperationHandle> LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsOperationHandle>();
            var assetOperationHandle = YooAssets.LoadSubAssetsAsync(assetInfo);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public UniTask<SubAssetsOperationHandle> LoadSubAssetsAsync(string path, Type type)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsOperationHandle>();
            var assetOperationHandle = YooAssets.LoadSubAssetsAsync(path, type);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public UniTask<SubAssetsOperationHandle> LoadSubAssetsAsync<T>(string path) where T : Object
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsOperationHandle>();
            var assetOperationHandle = YooAssets.LoadSubAssetsAsync<T>(path);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 异步加载子资源对象

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public UniTask<SubAssetsOperationHandle> LoadSubAssetsSync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsOperationHandle>();
            var assetOperationHandle = YooAssets.LoadSubAssetsSync(assetInfo);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public UniTask<SubAssetsOperationHandle> LoadSubAssetsSync(string path, Type type)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsOperationHandle>();
            var assetOperationHandle = YooAssets.LoadSubAssetsSync(path, type);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public UniTask<SubAssetsOperationHandle> LoadSubAssetsSync<T>(string path) where T : Object
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsOperationHandle>();
            var assetOperationHandle = YooAssets.LoadSubAssetsSync<T>(path);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 异步加载原生文件

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public UniTask<RawFileOperationHandle> LoadRawFileAsync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new UniTaskCompletionSource<RawFileOperationHandle>();
            var assetOperationHandle = YooAssets.LoadRawFileAsync(assetInfo);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public UniTask<RawFileOperationHandle> LoadRawFileAsync(string path)
        {
            var taskCompletionSource = new UniTaskCompletionSource<RawFileOperationHandle>();
            var assetOperationHandle = YooAssets.LoadRawFileAsync(path);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 同步加载原生文件

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public RawFileOperationHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            return YooAssets.LoadRawFileSync(assetInfo);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public RawFileOperationHandle LoadRawFileSync(string path)
        {
            return YooAssets.LoadRawFileSync(path);
        }

        #endregion


        #region 异步加载资源

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public UniTask<AssetOperationHandle> LoadAssetAsync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new UniTaskCompletionSource<AssetOperationHandle>();
            var assetOperationHandle = YooAssets.LoadAssetAsync(assetInfo);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public UniTask<AssetOperationHandle> LoadAssetAsync(string path, Type type)
        {
            var taskCompletionSource = new UniTaskCompletionSource<AssetOperationHandle>();
            var assetOperationHandle = YooAssets.LoadAssetAsync(path, type);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        public UniTask<AssetOperationHandle> LoadAssetAsync<T>(string path) where T : Object
        {
            var taskCompletionSource = new UniTaskCompletionSource<AssetOperationHandle>();
            var assetOperationHandle = YooAssets.LoadAssetAsync<T>(path);
            assetOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 同步加载资源

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public AssetOperationHandle LoadAssetSync(string path, Type type)
        {
            return YooAssets.LoadAssetSync(path, type);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public AssetOperationHandle LoadAssetSync(AssetInfo assetInfo)
        {
            return YooAssets.LoadAssetSync(assetInfo);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public AssetOperationHandle LoadAssetSync<T>(string path) where T : Object
        {
            return YooAssets.LoadAssetSync<T>(path);
        }

        #endregion

        #region 加载场景

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        public UniTask<SceneOperationHandle> LoadSceneAsync(string path, LoadSceneMode sceneMode,
            bool activateOnLoad = true)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SceneOperationHandle>();
            var sceneOperationHandle = YooAssets.LoadSceneAsync(path, sceneMode, activateOnLoad);
            sceneOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        public UniTask<SceneOperationHandle> LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode,
            bool activateOnLoad = true)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SceneOperationHandle>();
            var sceneOperationHandle = YooAssets.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad);
            sceneOperationHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 资源包

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public AssetsPackage CreateAssetsPackage(string packageName)
        {
            return YooAssets.CreateAssetsPackage(packageName);
        }

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public AssetsPackage TryGetAssetsPackage(string packageName)
        {
            return YooAssets.TryGetAssetsPackage(packageName);
        }

        /// <summary>
        /// 检查资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public bool HasAssetsPackage(string packageName)
        {
            return YooAssets.HasAssetsPackage(packageName);
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public AssetsPackage GetAssetsPackage(string packageName)
        {
            return YooAssets.GetAssetsPackage(packageName);
        }

        #endregion

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public bool IsNeedDownload(AssetInfo assetInfo)
        {
            return YooAssets.IsNeedDownloadFromRemote(assetInfo);
        }

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="path">资源地址</param>
        /// <returns></returns>
        public bool IsNeedDownload(string path)
        {
            return YooAssets.IsNeedDownloadFromRemote(path);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTags">资源标签列表</param>
        /// <returns></returns>
        public AssetInfo[] GetAssetInfos(string[] assetTags)
        {
            return YooAssets.GetAssetInfos(assetTags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTag">资源标签</param>
        /// <returns></returns>
        public AssetInfo[] GetAssetInfos(string assetTag)
        {
            return YooAssets.GetAssetInfos(assetTag);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        public AssetInfo GetAssetInfo(string path)
        {
            return YooAssets.GetAssetInfo(path);
        }

        /// <summary>
        /// 获取缓存目录根路径
        /// </summary>
        /// <returns></returns>
        public string GetCacheRootPath()
        {
            return YooAssets.GetSandboxRoot();
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void ClearCache()
        {
            YooAssets.ClearSandbox();
        }

        /// <summary>
        /// 设置默认资源包
        /// </summary>
        /// <param name="assetsPackage">资源信息</param>
        /// <returns></returns>
        public void SetDefaultAssetsPackage(AssetsPackage assetsPackage)
        {
            YooAssets.SetDefaultAssetsPackage(assetsPackage);
        }

        /// <summary>
        /// 销毁资源
        /// </summary>
        private void OnDestroy()
        {
            YooAssets.Destroy();
        }
    }
}