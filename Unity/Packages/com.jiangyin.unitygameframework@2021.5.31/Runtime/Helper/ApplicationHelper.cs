namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 应用帮助类
    /// </summary>
    public static class ApplicationHelper
    {
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