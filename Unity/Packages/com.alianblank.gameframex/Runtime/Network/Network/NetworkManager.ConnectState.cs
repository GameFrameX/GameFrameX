//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace GameFrameX.Network
{
    public sealed partial class NetworkManager
    {
        public sealed class ConnectState
        {
            private readonly INetworkSocket _socket;
            private readonly object _userData;

            public ConnectState(INetworkSocket socket, object userData)
            {
                _socket = socket;
                _userData = userData;
            }

            /// <summary>
            /// Socket
            /// </summary>
            public INetworkSocket Socket
            {
                get { return _socket; }
            }

            /// <summary>
            /// 用户自定义数据
            /// </summary>
            public object UserData
            {
                get { return _userData; }
            }
        }
    }
}