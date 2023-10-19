using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 全局配置组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Global Config")]
    public sealed class GlobalConfigComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 检测App版本地址接口
        /// </summary>
        public string CheckAppVersionUrl { get; set; } = string.Empty;

        /// <summary>
        /// 检测资源版本地址接口
        /// </summary>
        public string CheckResourceVersionUrl { get; set; } = string.Empty;

        /// <summary>
        /// 附加内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 主机服务地址
        /// </summary>
        public string HostServerUrl { get; set; } = string.Empty;
    }
}