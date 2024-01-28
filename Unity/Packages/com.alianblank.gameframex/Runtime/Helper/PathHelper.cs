using UnityEngine;

namespace GameFrameX.Runtime
{
    public static class PathHelper
    {
        /// <summary>
        ///应用程序外部资源路径存放路径(热更新资源路径)
        /// </summary>
        public static string AppHotfixResPath
        {
            get
            {
                string game = Application.productName;
                string path = $"{Application.persistentDataPath}/{game}/";
                return path;
            }
        }

        /// <summary>
        /// 应用程序内部资源路径存放路径
        /// </summary>
        public static string AppResPath
        {
            get { return NormalizePath(Application.streamingAssetsPath); }
        }

        /// <summary>
        /// 应用程序内部资源路径存放路径(www/webrequest专用)
        /// </summary>
        public static string AppResPath4Web
        {
            get
            {
#if UNITY_IOS || UNITY_STANDALONE_OSX
                return $"file://{Application.streamingAssetsPath}";
#else
                return NormalizePath(Application.streamingAssetsPath);
#endif
            }
        }

        /// <summary>
        /// 获取平台名称
        /// </summary>
        public static string GetPlatformName
        {
            get
            {
#if UNITY_ANDROID
                return $"Android";
#elif UNITY_STANDALONE_OSX
                return $"MacOs";
#elif UNITY_IOS || UNITY_IPHONE
                return $"iOS";
#elif UNITY_WEBGL
                return $"WebGL";
#elif UNITY_STANDLONE_WIN
                return $"Windows";
#else
                return string.Empty;
#endif
            }
        }

        /// <summary>
        /// 规范化路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Replace("\\", "/");
        }
    }
}