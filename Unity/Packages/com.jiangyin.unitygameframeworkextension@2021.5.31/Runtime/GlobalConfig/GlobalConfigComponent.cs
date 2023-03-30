using UnityEngine;
using UnityGameFramework.Runtime;

namespace GlobalConfig
{
    /// <summary>
    /// 全局配置组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Global Config")]
    public sealed class GlobalConfigComponent : GameFrameworkComponent
    {
        public string CheckAppVersionUrl { get; set; }
        public string CheckResourceVersionUrl { get; set; }
        public string Content { get; set; }
        public string HostServerUrl { get; set; }
    }
}