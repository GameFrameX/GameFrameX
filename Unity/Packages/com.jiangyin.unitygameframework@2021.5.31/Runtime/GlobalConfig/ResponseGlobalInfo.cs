namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 全局信息响应对象
    /// </summary>
    public sealed class ResponseGlobalInfo
    {
        /// <summary>
        /// 检测程序版本地址
        /// </summary>
        public string CheckAppVersionUrl { get; set; }

        /// <summary>
        /// 检测资源版本地址
        /// </summary>
        public string CheckResourceVersionUrl { get; set; }

        /// <summary>
        /// 扩展内容
        /// </summary>
        public string Content { get; set; }
    }
}