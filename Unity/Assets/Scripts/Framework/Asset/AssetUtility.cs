namespace Framework.Asset
{
    /// <summary>
    /// AB实用函数集，主要是路径拼接
    /// </summary>
    public static class AssetUtility
    {
        private const string BundlesPath = "Assets/Bundles";
        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetConfigPath(string fileName, string extension = ".bytes")
        {
            return $"{BundlesPath}/Config/{fileName}{extension}";
        }

        /// <summary>
        /// 获取代码文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetCodePath(string fileName, string extension = ".bytes")
        {
            return $"{BundlesPath}/Code/{fileName}{extension}";
        }

        /// <summary>
        /// 获取UI文件路径
        /// </summary>
        /// <param name="uiPackageName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetUIPackagePath(string uiPackageName, string extension = ".bytes")
        {
            return $"{BundlesPath}/UI/{uiPackageName}/{uiPackageName}";
        }

        /// <summary>
        /// 获取声音文件路径
        /// </summary>
        /// <param name="soundName">声音名称</param>
        /// <param name="extension">扩展名</param>
        /// <returns></returns>
        public static string GetSoundPath(string soundName, string extension = ".mp3")
        {
            return $"{BundlesPath}/Sound/{soundName}.{extension}";
        }
    }
}