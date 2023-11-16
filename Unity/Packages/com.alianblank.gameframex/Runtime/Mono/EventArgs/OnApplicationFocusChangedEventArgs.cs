//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX.Event;

namespace GameFrameX.Mono
{
    /// <summary>
    /// 程序前后台切换事件。
    /// </summary>
    public sealed class OnApplicationFocusChangedEventArgs : GameEventArgs
    {
        /// <summary>
        /// 加载字典更新事件编号。
        /// </summary>
        public static readonly string EventId = typeof(OnApplicationFocusChangedEventArgs).FullName;

        /// <summary>
        /// 初始化加载字典更新事件的新实例。
        /// </summary>
        public OnApplicationFocusChangedEventArgs()
        {
            IsFocus = false;
        }

        /// <summary>
        /// 获取加载字典更新事件编号。
        /// </summary>
        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 是否是前台。
        /// </summary>
        public bool IsFocus { get; private set; }

        /// <summary>
        /// 创建加载字典更新事件。
        /// </summary>
        /// <param name="isFocus">是否是前台</param>
        /// <returns>创建的加载字典更新事件。</returns>
        public static OnApplicationFocusChangedEventArgs Create(bool isFocus)
        {
            var loadDictionaryUpdateChangedEventArgs = ReferencePool.Acquire<OnApplicationFocusChangedEventArgs>();
            loadDictionaryUpdateChangedEventArgs.IsFocus = isFocus;
            return loadDictionaryUpdateChangedEventArgs;
        }

        /// <summary>
        /// 清理前后台切换事件。
        /// </summary>
        public override void Clear()
        {
            IsFocus = false;
        }
    }
}