using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 应用帮助类
    /// </summary>
    public static class ApplicationHelper
    {
        /// <summary>
        /// 是否是编辑器
        /// </summary>
        public static bool IsEditor
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// 是否是安卓
        /// </summary>
        public static bool IsAndroid
        {
            get
            {
#if UNITY_ANDROID
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// 是否是WebGL平台
        /// </summary>
        public static bool IsWebGL
        {
            get { return Application.platform == RuntimePlatform.WebGLPlayer; }
        }

        /// <summary>
        /// 是否是Windows平台
        /// </summary>
        public static bool IsWindows
        {
            get { return Application.platform == RuntimePlatform.WindowsPlayer; }
        }

        /// <summary>
        /// 是否是Linux平台
        /// </summary>
        public static bool IsLinux
        {
            get { return Application.platform == RuntimePlatform.LinuxPlayer; }
        }


        /// <summary>
        /// 是否是Mac平台
        /// </summary>
        public static bool IsMacOsx
        {
            get { return Application.platform == RuntimePlatform.OSXPlayer; }
        }

        /// <summary>
        /// 是否是iOS 移动平台
        /// </summary>
        public static bool IsIOS
        {
            get
            {
#if UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }
    }
}