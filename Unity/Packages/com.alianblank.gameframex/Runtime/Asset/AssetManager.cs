using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    public sealed class AssetManager : GameFrameworkModule, IAssetManager
    {
        private EPlayMode _gamePlayMode;
        private ResourcePackage _buildinPackage;
        public const string BuildInPackageName = "DefaultPackage";

        private InitializationOperation _initializationOperation;
        private string _hostServer;
        public string StaticVersion { get; private set; }


        /// <summary>
        /// 更新静态版本
        /// </summary>
        /// <param name="version"></param>
        public void UpdateStaticVersion(string version)
        {
            StaticVersion = version;
        }

        /// <summary>
        /// 设置运行模式
        /// </summary>
        /// <param name="playMode">运行模式</param>
        public void SetPlayMode(EPlayMode playMode)
        {
            _gamePlayMode = playMode;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public async UniTask Initialize(string host)
        {
            _hostServer = host;
            Debug.Log($"资源系统运行模式：{_gamePlayMode}");
            YooAssets.Initialize();
            YooAssets.SetOperationSystemMaxTimeSlice(30);
            // YooAssets.SetCacheSystemCachedFileVerifyLevel(EVerifyLevel.High);
            // YooAssets.SetDownloadSystemBreakpointResumeFileSize(4096 * 8);

            // 创建默认的资源包
            _buildinPackage = YooAssets.TryGetPackage(BuildInPackageName);
            if (_buildinPackage == null)
            {
                _buildinPackage = YooAssets.CreatePackage(BuildInPackageName);
                // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
                YooAssets.SetDefaultPackage(_buildinPackage);
            }

            if (_gamePlayMode == EPlayMode.EditorSimulateMode)
            {
                // 编辑器下的模拟模式
                InitializeYooAssetEditorSimulateMode();
            }
            else if (_gamePlayMode == EPlayMode.OfflinePlayMode)
            {
                // 单机运行模式
                InitializeYooAssetOfflinePlayMode();
            }
            else if (_gamePlayMode == EPlayMode.HostPlayMode)
            {
                // 联机运行模式
                InitializeYooAssetHostPlayMode();
            }
            else if (_gamePlayMode == EPlayMode.WebPlayMode)
            {
                // WebGL运行模式
                InitializeYooAssetWebPlayMode();
            }

            await _initializationOperation.ToUniTask();

            Debug.Log("Asset Init Over");
        }

        private void InitializeYooAssetEditorSimulateMode()
        {
            var initParameters = new EditorSimulateModeParameters();
            initParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(), BuildInPackageName);
            
            _initializationOperation = _buildinPackage.InitializeAsync(initParameters);
        }

        private void InitializeYooAssetOfflinePlayMode()
        {
            var initParameters = new OfflinePlayModeParameters();
            _initializationOperation = _buildinPackage.InitializeAsync(initParameters);
        }

        private void InitializeYooAssetWebPlayMode()
        {
            var initParameters = new WebPlayModeParameters();
            initParameters.BuildinQueryServices = new QueryStreamingAssetsFileServices();
            initParameters.RemoteServices = new RemoteServices(_hostServer, _hostServer);
            _initializationOperation = _buildinPackage.InitializeAsync(initParameters);
        }

        private void InitializeYooAssetHostPlayMode()
        {
            var initParameters = new HostPlayModeParameters();
            initParameters.BuildinQueryServices = new QueryStreamingAssetsFileServices();
            initParameters.RemoteServices = new RemoteServices(_hostServer, _hostServer);
            // initParameters.DeliveryQueryServices = new WebDeliveryQueryServices();
            // initParameters.DeliveryLoadServices = new WebDeliveryLoadServices();
            _initializationOperation = _buildinPackage.InitializeAsync(initParameters);
        }

        private class RemoteServices : IRemoteServices
        {
            public string HostServer { get; }
            public string FallbackHostServer { get; }

            public RemoteServices(string hostServer, string fallbackHostServer)
            {
                HostServer = hostServer;
                FallbackHostServer = fallbackHostServer;
            }

            public string GetRemoteMainURL(string fileName)
            {
                return HostServer + fileName;
            }

            public string GetRemoteFallbackURL(string fileName)
            {
                return FallbackHostServer + fileName;
            }
        }

        /// <summary>
        /// 内置文件查询服务类
        /// </summary>
        private class QueryStreamingAssetsFileServices : IBuildinQueryServices
        {
            public bool Query(string packageName, string fileName)
            {
                // 注意：使用了BetterStreamingAssets插件，使用前需要初始化该插件！
                string buildinFolderName = PathHelper.AppResPath;
                return BetterStreamingAssets.FileExists($"{buildinFolderName}/{fileName}");
            }
        }

        #region 异步加载子资源对象

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public UniTask<SubAssetsHandle> LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsHandle>();
            var assetHandle = YooAssets.LoadSubAssetsAsync(assetInfo);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public UniTask<SubAssetsHandle> LoadSubAssetsAsync(string path, Type type)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsHandle>();
            var assetHandle = YooAssets.LoadSubAssetsAsync(path, type);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public UniTask<SubAssetsHandle> LoadSubAssetsAsync<T>(string path) where T : Object
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsHandle>();
            var assetHandle = YooAssets.LoadSubAssetsAsync<T>(path);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 异步加载子资源对象

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public UniTask<SubAssetsHandle> LoadSubAssetsSync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsHandle>();
            var assetHandle = YooAssets.LoadSubAssetsSync(assetInfo);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public UniTask<SubAssetsHandle> LoadSubAssetsSync(string path, Type type)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsHandle>();
            var assetHandle = YooAssets.LoadSubAssetsSync(path, type);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public UniTask<SubAssetsHandle> LoadSubAssetsSync<T>(string path) where T : Object
        {
            var taskCompletionSource = new UniTaskCompletionSource<SubAssetsHandle>();
            var assetHandle = YooAssets.LoadSubAssetsSync<T>(path);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 异步加载原生文件

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public UniTask<RawFileHandle> LoadRawFileAsync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new UniTaskCompletionSource<RawFileHandle>();
            var assetHandle = YooAssets.LoadRawFileAsync(assetInfo);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public UniTask<RawFileHandle> LoadRawFileAsync(string path)
        {
            var taskCompletionSource = new UniTaskCompletionSource<RawFileHandle>();
            var assetHandle = YooAssets.LoadRawFileAsync(path);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 同步加载原生文件

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public RawFileHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            return YooAssets.LoadRawFileSync(assetInfo);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public RawFileHandle LoadRawFileSync(string path)
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
        public UniTask<AssetHandle> LoadAssetAsync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new UniTaskCompletionSource<AssetHandle>();
            var assetHandle = YooAssets.LoadAssetAsync(assetInfo);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public UniTask<AssetHandle> LoadAssetAsync(string path, Type type)
        {
            var taskCompletionSource = new UniTaskCompletionSource<AssetHandle>();
            var assetHandle = YooAssets.LoadAssetAsync(path, type);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        public UniTask<AssetHandle> LoadAssetAsync<T>(string path) where T : Object
        {
            var taskCompletionSource = new UniTaskCompletionSource<AssetHandle>();
            var assetHandle = YooAssets.LoadAssetAsync<T>(path);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
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
        public AssetHandle LoadAssetSync(string path, Type type)
        {
            return YooAssets.LoadAssetSync(path, type);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public AssetHandle LoadAssetSync(AssetInfo assetInfo)
        {
            return YooAssets.LoadAssetSync(assetInfo);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public AssetHandle LoadAssetSync<T>(string path) where T : Object
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
        public UniTask<SceneHandle> LoadSceneAsync(string path, LoadSceneMode sceneMode,
            bool activateOnLoad = true)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SceneHandle>();
            var sceneHandle = YooAssets.LoadSceneAsync(path, sceneMode, activateOnLoad);
            sceneHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        public UniTask<SceneHandle> LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode,
            bool activateOnLoad = true)
        {
            var taskCompletionSource = new UniTaskCompletionSource<SceneHandle>();
            var sceneHandle = YooAssets.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad);
            sceneHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 资源包

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public ResourcePackage CreateAssetsPackage(string packageName)
        {
            return YooAssets.CreatePackage(packageName);
        }

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public ResourcePackage TryGetAssetsPackage(string packageName)
        {
            return YooAssets.TryGetPackage(packageName);
        }

        /// <summary>
        /// 检查资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public bool HasAssetsPackage(string packageName)
        {
            return YooAssets.TryGetPackage(packageName) != null;
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public ResourcePackage GetAssetsPackage(string packageName)
        {
            return YooAssets.GetPackage(packageName);
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
        /// 设置默认资源包
        /// </summary>
        /// <param name="resourcePackage">资源信息</param>
        /// <returns></returns>
        public void SetDefaultAssetsPackage(ResourcePackage resourcePackage)
        {
            YooAssets.SetDefaultPackage(resourcePackage);
        }

        /// <summary>
        /// 销毁资源
        /// </summary>
        public void OnDestroy()
        {
            YooAssets.Destroy();
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            OnDestroy();
        }
    }
}