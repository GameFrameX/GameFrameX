//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX.Event;

namespace GameFrameX.Download
{
    /// <summary>
    /// 下载代理辅助器完成事件。
    /// </summary>
    public sealed class DownloadAgentHelperCompleteEventArgs : GameEventArgs
    {
        public static readonly string EventId = typeof(DownloadAgentHelperCompleteEventArgs).FullName;

        /// <summary>
        /// 初始化下载代理辅助器完成事件的新实例。
        /// </summary>
        public DownloadAgentHelperCompleteEventArgs()
        {
            Length = 0L;
        }

        /// <summary>
        /// 获取下载的数据大小。
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// 创建下载代理辅助器完成事件。
        /// </summary>
        /// <param name="length">下载的数据大小。</param>
        /// <returns>创建的下载代理辅助器完成事件。</returns>
        public static DownloadAgentHelperCompleteEventArgs Create(long length)
        {
            if (length < 0L)
            {
                throw new GameFrameworkException("Length is invalid.");
            }

            DownloadAgentHelperCompleteEventArgs downloadAgentHelperCompleteEventArgs = ReferencePool.Acquire<DownloadAgentHelperCompleteEventArgs>();
            downloadAgentHelperCompleteEventArgs.Length = length;
            return downloadAgentHelperCompleteEventArgs;
        }

        /// <summary>
        /// 清理下载代理辅助器完成事件。
        /// </summary>
        public override void Clear()
        {
            Length = 0L;
        }

        public override string Id
        {
            get { return EventId; }
        }
    }
}