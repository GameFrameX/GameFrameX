/*//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;
using GameFrameX.Network.Pipelines;
using GameFrameX.Network.Pipelines.Protocols;

namespace GameFrameX.Network
{
    /// <summary>
    /// IO Pipeline 网络频道。
    /// </summary>
    sealed class IOPipelineNetworkChannel : NetworkManager.NetworkChannelBase
    {
        private readonly AsyncCallback m_ConnectCallback;
        private readonly AsyncCallback m_SendCallback;
        private readonly AsyncCallback m_ReceiveCallback;

        IProtoCalWriteHelper<MessageObject> _protoCalWriteHelper;
        IProtoCalReadHelper<MessageObject> _protoCalReadHelper;

        /// <summary>
        /// 初始化网络频道的新实例。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <param name="networkChannelHelper">网络频道辅助器。</param>
        public IOPipelineNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
            : base(name, networkChannelHelper)
        {
            // m_ConnectCallback = ConnectCallback;
            m_SendCallback = SendCallback;
            m_ReceiveCallback = ReceiveCallback;
            _protoCalWriteHelper = new ClientProtocolWriteHelper();
            _protoCalReadHelper = new ClientProtocolReadHelper();
        }

        private SocketConnection socketConnection;
        private NetworkManager.ConnectState connectState;

        /// <summary>
        /// 连接到远程主机。
        /// </summary>
        /// <param name="ipAddress">远程主机的 IP 地址。</param>
        /// <param name="port">远程主机的端口号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public override async void Connect(IPAddress ipAddress, int port, object userData)
        {
            base.Connect(ipAddress, port, userData);
            var (ipType, host) = Utility.Net.GetIPv6Address(ipAddress.ToString(), port);
            socketConnection = new SocketConnection(ipAddress.AddressFamily, host, port);
            connectState = new NetworkManager.ConnectState(socketConnection.Socket, userData);

            var context = await socketConnection.StartAsync(0);

            if (context != null)
            {
                if (NetworkChannelConnected != null)
                {
                    NetworkChannelConnected(this, connectState.UserData);
                }

                var channel = new NetChannel(context, _protoCalWriteHelper, _protoCalReadHelper, ReceiveCallback, ConnectCallback);
                // _ = channel.StartReadMsgAsync();
            }
            else
            {
                string errorMessage = "Initialize network channel failure.";
                if (NetworkChannelError != null)
                {
                    NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.Success, errorMessage);
                    return;
                }

                throw new GameFrameworkException(errorMessage);
            }

            PNetworkChannelHelper.PrepareForConnecting();
            ConnectAsync(ipAddress, port, userData);
        }

        private void ReceiveCallback(MessageObject obj)
        {
        }

        public void OnDisConnected()
        {
            // var rMsg = new NetMessage();
            // rMsg.MsgId = DisconnectEvt;
            // rMsg.Msg = NetCode.Closed;
            // msgQueue.Enqueue(rMsg);
        }

        public void Close(bool triggerCloseEvt)
        {
            // channel?.Abort(triggerCloseEvt);
            // channel = null;
            // ClearAllMsg();
        }

        protected override bool ProcessSend()
        {
            if (base.ProcessSend())
            {
                SendAsync();
                return true;
            }

            return false;
        }

        private void ConnectAsync(IPAddress ipAddress, int port, object userData)
        {
            try
            {
                PSocket.BeginConnect(ipAddress, port, m_ConnectCallback, new NetworkManager.ConnectState(PSocket, userData));
            }
            catch (Exception exception)
            {
                if (NetworkChannelError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                    return;
                }

                throw;
            }
        }

        private void ConnectCallback(NetChannel netChannel)
        {
            PSentPacketCount = 0;
            PReceivedPacketCount = 0;

            lock (PSendPacketPool)
            {
                PSendPacketPool.Clear();
            }

            PReceivePacketPool.Clear();

            lock (PHeartBeatState)
            {
                PHeartBeatState.Reset(true);
            }

            if (NetworkChannelConnected != null)
            {
                NetworkChannelConnected(this, connectState.UserData);
            }

            PActive = true;
            _ = netChannel.StartReadMsgAsync();
        }

        private void SendAsync()
        {
            try
            {
                PSocket.BeginSend(PSendState.Stream.GetBuffer(), (int)PSendState.Stream.Position, (int)(PSendState.Stream.Length - PSendState.Stream.Position), SocketFlags.None, m_SendCallback, PSocket);
            }
            catch (Exception exception)
            {
                PActive = false;
                if (NetworkChannelError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetworkChannelError(this, NetworkErrorCode.SendError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                    return;
                }

                throw;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            if (!socket.Connected)
            {
                return;
            }

            int bytesSent = 0;
            try
            {
                bytesSent = socket.EndSend(ar);
            }
            catch (Exception exception)
            {
                PActive = false;
                if (NetworkChannelError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                    return;
                }

                throw;
            }

            PSendState.Stream.Position += bytesSent;
            if (PSendState.Stream.Position < PSendState.Stream.Length)
            {
                SendAsync();
                return;
            }

            PSentPacketCount++;
            PSendState.Reset();
        }

        private void ReceiveAsync()
        {
            try
            {
                PSocket.BeginReceive(PReceiveState.Stream.GetBuffer(), (int)PReceiveState.Stream.Position, (int)(PReceiveState.Stream.Length - PReceiveState.Stream.Position), SocketFlags.None, m_ReceiveCallback, PSocket);
            }
            catch (Exception exception)
            {
                PActive = false;
                if (NetworkChannelError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                    return;
                }

                throw;
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            if (!socket.Connected)
            {
                return;
            }

            int bytesReceived = 0;
            try
            {
                bytesReceived = socket.EndReceive(ar);
            }
            catch (Exception exception)
            {
                PActive = false;
                if (NetworkChannelError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                    return;
                }

                throw;
            }

            if (bytesReceived <= 0)
            {
                Close();
                return;
            }

            PReceiveState.Stream.Position += bytesReceived;
            if (PReceiveState.Stream.Position < PReceiveState.Stream.Length)
            {
                ReceiveAsync();
                return;
            }

            PReceiveState.Stream.Position = 0L;

            bool processSuccess = false;
            if (PReceiveState.PacketHeader != null)
            {
                processSuccess = ProcessPacket();
                PReceivedPacketCount++;
            }
            else
            {
                processSuccess = ProcessPacketHeader();
            }

            if (processSuccess)
            {
                ReceiveAsync();
                return;
            }
        }
    }
}*/