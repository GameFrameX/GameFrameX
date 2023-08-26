namespace Framework.Asset
{
    /// <summary>
    /// AB实用函数集，主要是路径拼接
    /// </summary>
    public static class AssetUtility
    {
        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetConfigPath(string fileName, string extension = ".bytes")
        {
            return $"Assets/Bundles/Config/{fileName}{extension}";
        }

        /// <summary>
        /// 获取代码文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetCodePath(string fileName, string extension = ".bytes")
        {
            return $"Assets/Bundles/Code/{fileName}{extension}";
        }

        /// <summary>
        /// 获取UI文件路径
        /// </summary>
        /// <param name="uiPackageName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetUIPackagePath(string uiPackageName, string extension = ".bytes")
        {
            return $"Assets/Bundles/UI/{uiPackageName}/{uiPackageName}";
        }
    }
}