//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX.Event;

namespace GameFrameX.Config
{
    /// <summary>
    /// 加载全局配置更新事件。
    /// </summary>
    public sealed class LoadConfigUpdateEventArgs : GameEventArgs
    {
        /// <summary>
        /// 加载全局配置失败事件编号。
        /// </summary>
        public static readonly string EventId = typeof(LoadConfigUpdateEventArgs).FullName;

        /// <summary>
        /// 初始化加载全局配置更新事件的新实例。
        /// </summary>
        public LoadConfigUpdateEventArgs()
        {
            ConfigAssetName = null;
            Progress = 0f;
            UserData = null;
        }

        /// <summary>
        /// 获取加载全局配置失败事件编号。
        /// </summary>
        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 获取全局配置资源名称。
        /// </summary>
        public string ConfigAssetName { get; private set; }

        /// <summary>
        /// 获取加载全局配置进度。
        /// </summary>
        public float Progress { get; private set; }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData { get; private set; }

        /// <summary>
        /// 创建加载全局配置更新事件。
        /// </summary>
        /// <param name="dataAssetName"></param>
        /// <param name="progress"></param>
        /// <param name="userData"></param>
        /// <returns>创建的加载全局配置更新事件。</returns>
        public static LoadConfigUpdateEventArgs Create(string dataAssetName, float progress, object userData)
        {
            LoadConfigUpdateEventArgs loadConfigUpdateEventArgs = ReferencePool.Acquire<LoadConfigUpdateEventArgs>();
            loadConfigUpdateEventArgs.ConfigAssetName = dataAssetName;
            loadConfigUpdateEventArgs.Progress = progress;
            loadConfigUpdateEventArgs.UserData = userData;
            return loadConfigUpdateEventArgs;
        }

        /// <summary>
        /// 清理加载全局配置更新事件。
        /// </summary>
        public override void Clear()
        {
            ConfigAssetName = null;
            Progress = 0f;
            UserData = null;
        }
    }
}