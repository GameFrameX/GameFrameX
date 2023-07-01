//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace GameFramework.Network
{
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// 链接状态
        /// </summary>
        private sealed class ConnectState
        {
            public ConnectState(SocketConnection socketConnection, object userData)
            {
                SocketConnection = socketConnection;
                UserData = userData;
            }

            /// <summary>
            /// Socket 链接对象
            /// </summary>
            public SocketConnection SocketConnection { get; }

            /// <summary>
            /// 用户自定义数据
            /// </summary>
            public object UserData { get; }
        }
    }
}