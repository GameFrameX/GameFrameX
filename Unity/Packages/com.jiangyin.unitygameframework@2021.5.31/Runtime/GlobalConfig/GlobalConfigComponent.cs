using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 全局配置组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Global Config")]
    public sealed class GlobalConfigComponent : GameFrameworkComponent
    {
        public string CheckAppVersionUrl { get; set; } = string.Empty;
        public string CheckResourceVersionUrl { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string HostServerUrl { get; set; } = string.Empty;
    }
}