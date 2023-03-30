namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// UI层级
    /// </summary>
    public enum UILayer
    {
        /// <summary>
        /// 隐藏
        /// </summary>
        Hidden = 20,

        /// <summary>
        /// 底板
        /// </summary>
        Floor = 15,

        /// <summary>
        /// 正常
        /// </summary>
        Normal = 10,

        /// <summary>
        /// 固定
        /// </summary>
        Fixed = 0,

        /// <summary>
        /// 窗口
        /// </summary>
        Window = -10,

        /// <summary>
        /// 提示
        /// </summary>
        Tip = -15,


        /// <summary>
        /// 引导
        /// </summary>
        Guide = -20,

        /// <summary>
        /// 引导
        /// </summary>
        BlackBoard = -22,

        /// <summary>
        /// 引导
        /// </summary>
        Dialogue = -23,

        /// <summary>
        /// Loading 
        /// </summary>
        Loading = -25,

        /// <summary>
        /// 通知
        /// </summary>
        Notify = -30,

        /// <summary>
        /// 系统顶级
        /// </summary>
        System = -35,
    }
}