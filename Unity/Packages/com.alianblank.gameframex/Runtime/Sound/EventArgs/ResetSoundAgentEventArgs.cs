//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX.Event;

namespace GameFrameX.Sound
{
    /// <summary>
    /// 重置声音代理事件。
    /// </summary>
    public sealed class ResetSoundAgentEventArgs : GameEventArgs
    {
        /// <summary>
        /// 初始化重置声音代理事件的新实例。
        /// </summary>
        public ResetSoundAgentEventArgs()
        {
        }

        /// <summary>
        /// 创建重置声音代理事件。
        /// </summary>
        /// <returns>创建的重置声音代理事件。</returns>
        public static ResetSoundAgentEventArgs Create()
        {
            ResetSoundAgentEventArgs resetSoundAgentEventArgs = ReferencePool.Acquire<ResetSoundAgentEventArgs>();
            return resetSoundAgentEventArgs;
        }

        /// <summary>
        /// 清理重置声音代理事件。
        /// </summary>
        public override void Clear()
        {
        }

        /// <summary>
        /// 播放声音更新事件编号。
        /// </summary>
        public static readonly string EventId = typeof(ResetSoundAgentEventArgs).FullName;

        public override string Id
        {
            get { return EventId; }
        }
    }
}