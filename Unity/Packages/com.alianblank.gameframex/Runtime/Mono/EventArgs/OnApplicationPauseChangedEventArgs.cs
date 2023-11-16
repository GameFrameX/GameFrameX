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
    /// 程序是否是暂停状态变化事件。
    /// </summary>
    public sealed class OnApplicationPauseChangedEventArgs : GameEventArgs
    {
        /// <summary>
        /// 程序是否是暂停状态变化更新事件编号。
        /// </summary>
        public static readonly string EventId = typeof(OnApplicationPauseChangedEventArgs).FullName;

        /// <summary>
        /// 初始化加载字典更新事件的新实例。
        /// </summary>
        public OnApplicationPauseChangedEventArgs()
        {
            IsPause = false;
        }

        /// <summary>
        /// 获取加载字典更新事件编号。
        /// </summary>
        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 是否暂停。
        /// </summary>
        public bool IsPause { get; private set; }


        /// <summary>
        /// 创建加载字典更新事件。
        /// </summary>
        /// <param name="isPause">是否是前台</param>
        /// <returns>创建的加载字典更新事件。</returns>
        public static OnApplicationPauseChangedEventArgs Create(bool isPause)
        {
            var loadDictionaryUpdateEventArgs = ReferencePool.Acquire<OnApplicationPauseChangedEventArgs>();
            loadDictionaryUpdateEventArgs.IsPause = isPause;
            return loadDictionaryUpdateEventArgs;
        }

        /// <summary>
        /// 清理前后台切换事件。
        /// </summary>
        public override void Clear()
        {
            IsPause = false;
        }
    }
}