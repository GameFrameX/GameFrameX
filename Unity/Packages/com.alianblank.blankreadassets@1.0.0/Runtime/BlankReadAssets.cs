using UnityEngine;

namespace BlankReadAssets
{
    public static class BlankReadAssets
    {
        private static AndroidJavaClass _androidJavaClass;

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="path">相对目录</param>
        /// <returns></returns>
        public static byte[] Read(string path)
        {
            Guard();
            return _androidJavaClass.CallStatic<byte[]>("readFile", path);
        }

        private static void Guard()
        {
            if (_androidJavaClass == null)
            {
                _androidJavaClass = new AndroidJavaClass("com.alianblank.readassets.MainActivity");
            }
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="path">相对目录</param>
        /// <returns></returns>
        public static bool IsFileExists(string path)
        {
            Guard();

            return _androidJavaClass.CallStatic<bool>("isFileExists", path);
        }
    }
}