//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;
using GameFramework.Network;
using System.Net.Sockets;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 网络错误事件。
    /// </summary>
    public sealed class NetErrorEventArgs : GameEventArgs
    {
        /// <summary>
        /// 网络错误事件编号。
        /// </summary>
        public static readonly int EventId = typeof(NetErrorEventArgs).GetHashCode();

        /// <summary>
        /// 初始化网络错误事件的新实例。
        /// </summary>
        public NetErrorEventArgs()
        {
            NetworkChannel = null;
            ErrorCode = NetworkErrorCode.Unknown;
            ErrorMessage = null;
        }

        /// <summary>
        /// 获取网络错误事件编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        public INetworkChannel NetworkChannel
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取错误码。
        /// </summary>
        public NetworkErrorCode ErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取 Socket 错误码。
        /// </summary>
        public SocketError SocketErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取错误信息。
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建网络错误事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的网络错误事件。</returns>
        public static NetErrorEventArgs Create(GameFramework.Network.NetworkErrorEventArgs e)
        {
            NetErrorEventArgs networkErrorEventArgs = ReferencePool.Acquire<NetErrorEventArgs>();
            networkErrorEventArgs.NetworkChannel = e.NetworkChannel;
            networkErrorEventArgs.ErrorCode = e.ErrorCode;
            networkErrorEventArgs.SocketErrorCode = e.SocketErrorCode;
            networkErrorEventArgs.ErrorMessage = e.ErrorMessage;
            return networkErrorEventArgs;
        }

        /// <summary>
        /// 清理网络错误事件。
        /// </summary>
        public override void Clear()
        {
            NetworkChannel = null;
            ErrorCode = NetworkErrorCode.Unknown;
            ErrorMessage = null;
        }
    }
}
