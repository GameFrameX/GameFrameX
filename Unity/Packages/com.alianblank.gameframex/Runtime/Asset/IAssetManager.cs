using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace GameFrameX.Asset
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    public interface IAssetManager
    {
        /// <summary>
        /// 同时下载的最大数目。
        /// </summary>
        int DownloadingMaxNum { get; set; }

        /// <summary>
        /// 失败重试最大数目。
        /// </summary>
        int FailedTryAgain { get; set; }

        /// <summary>
        /// 获取资源只读区路径。
        /// </summary>
        string ReadOnlyPath { get; }

        /// <summary>
        /// 获取资源读写区路径。
        /// </summary>
        string ReadWritePath { get; }

        /// <summary>
        /// 设置资源只读区路径。
        /// </summary>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        void SetReadOnlyPath(string readOnlyPath);

        /// <summary>
        /// 设置资源读写区路径。
        /// </summary>
        /// <param name="readWritePath">资源读写区路径。</param>
        void SetReadWritePath(string readWritePath);

        /// <summary>
        /// 获取或设置资源包名称。
        /// </summary>
        string DefaultPackageName { get; set; }

        /// <summary>
        /// 获取或设置运行模式。
        /// </summary>
        EPlayMode PlayMode { get; }

        /// <summary>
        /// 获取或设置下载文件校验等级。
        /// </summary>
        EVerifyLevel VerifyLevel { get; }

        /// <summary>
        /// 获取或设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）。
        /// </summary>
        long Milliseconds { get; set; }

        /// <summary>
        /// 更新静态版本
        /// </summary>
        /// <param name="version"></param>
        void UpdateStaticVersion(string version);

        /// <summary>
        /// 设置运行模式
        /// </summary>
        /// <param name="playMode">运行模式</param>
        void SetPlayMode(EPlayMode playMode);

        /// <summary>
        /// 热更链接URL。
        /// </summary>
        string HostServerURL { get; }

        /// <summary>
        /// 备用热更链接URL。
        /// </summary>
        string FallbackHostServerURL { get; }

        /// <summary>
        /// 设置热更链接URL。
        /// </summary>
        /// <param name="hostServerURL">热更链接URL。</param>
        void SetHostServerURL(string hostServerURL);

        /// <summary>
        /// 设置备用热更链接URL
        /// </summary>
        /// <param name="fallbackHostServerURL"></param>
        void SetFallbackHostServerURL(string fallbackHostServerURL);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        void Initialize();

        /// <summary>
        /// 初始化操作。
        /// </summary>
        /// <returns></returns>
        InitializationOperation InitPackage();


        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="assetPath"></param>
        void UnloadAsset(string assetPath);

        #region 异步加载子资源对象

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        UniTask<SubAssetsHandle> LoadSubAssetsAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        UniTask<SubAssetsHandle> LoadSubAssetsAsync(string path, Type type);

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        UniTask<SubAssetsHandle> LoadSubAssetsAsync<T>(string path) where T : Object;

        #endregion

        #region 异步加载子资源对象

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        UniTask<SubAssetsHandle> LoadSubAssetsSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        UniTask<SubAssetsHandle> LoadSubAssetsSync(string path, Type type);

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        UniTask<SubAssetsHandle> LoadSubAssetsSync<T>(string path) where T : Object;

        #endregion

        #region 异步加载原生文件

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        UniTask<RawFileHandle> LoadRawFileAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        UniTask<RawFileHandle> LoadRawFileAsync(string path);

        #endregion

        #region 同步加载原生文件

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        RawFileHandle LoadRawFileSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        RawFileHandle LoadRawFileSync(string path);

        #endregion


        #region 异步加载资源

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        UniTask<AssetHandle> LoadAssetAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        UniTask<AssetHandle> LoadAssetAsync(string path, Type type);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        UniTask<AssetHandle> LoadAssetAsync<T>(string path) where T : Object;

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetPath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        System.Threading.Tasks.Task<T> LoadAssetTaskAsync<T>(string assetPath) where T : UnityEngine.Object;

        #endregion

        #region 同步加载资源

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        AssetHandle LoadAssetSync(string path, Type type);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        AssetHandle LoadAssetSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        AssetHandle LoadAssetSync<T>(string path) where T : Object;

        #endregion

        #region 加载场景

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        UniTask<SceneHandle> LoadSceneAsync(string path, LoadSceneMode sceneMode, bool activateOnLoad = true);

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        UniTask<SceneHandle> LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad = true);

        #endregion

        #region 资源包

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        ResourcePackage CreateAssetsPackage(string packageName);

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        ResourcePackage TryGetAssetsPackage(string packageName);

        /// <summary>
        /// 检查资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        bool HasAssetsPackage(string packageName);

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        ResourcePackage GetAssetsPackage(string packageName);

        #endregion

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        bool IsNeedDownload(AssetInfo assetInfo);

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="path">资源地址</param>
        /// <returns></returns>
        bool IsNeedDownload(string path);

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTags">资源标签列表</param>
        /// <returns></returns>
        AssetInfo[] GetAssetInfos(string[] assetTags);

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTag">资源标签</param>
        /// <returns></returns>
        AssetInfo[] GetAssetInfos(string assetTag);

        /// <summary>
        /// 获取资源信息
        /// </summary>
        AssetInfo GetAssetInfo(string path);

        /// <summary>
        /// 设置默认资源包
        /// </summary>
        /// <param name="resourcePackage">资源信息</param>
        /// <returns></returns>
        void SetDefaultAssetsPackage(ResourcePackage resourcePackage);

        /// <summary>
        /// 销毁资源
        /// </summary>
        void OnDestroy();
    }
}