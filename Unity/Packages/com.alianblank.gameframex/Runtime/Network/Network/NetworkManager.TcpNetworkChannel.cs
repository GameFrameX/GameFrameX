/*//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;

namespace GameFrameX.Network
{
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// TCP 网络频道。
        /// </summary>
        private sealed class TcpNetworkChannel : NetworkChannelBase
        {
            private readonly AsyncCallback _connectCallback;
            private readonly AsyncCallback _sendCallback;
            private readonly AsyncCallback _receiveCallback;

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public TcpNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
                _connectCallback = ConnectCallback;
                _sendCallback = SendCallback;
                _receiveCallback = ReceiveCallback;
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public override void Connect(IPAddress ipAddress, int port, object userData = null)
            {
                base.Connect(ipAddress, port, userData);
                PSocket = new SystemNetSocket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (PSocket == null)
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
                    ((SystemNetSocket)PSocket).BeginConnect(ipAddress, port, _connectCallback, new ConnectState(PSocket, userData));
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

            private void ConnectCallback(IAsyncResult ar)
            {
                ConnectState socketUserData = (ConnectState)ar.AsyncState;
                try
                {
                    ((SystemNetSocket)socketUserData.Socket).EndConnect(ar);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

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
                    NetworkChannelConnected(this, socketUserData.UserData);
                }

                PActive = true;
                ReceiveAsync();
            }

            private void SendAsync()
            {
                try
                {
                    ((SystemNetSocket)PSocket).BeginSend(PSendState.Stream.GetBuffer(), (int)PSendState.Stream.Position, (int)(PSendState.Stream.Length - PSendState.Stream.Position), SocketFlags.None, _sendCallback, PSocket);
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
            }

            private void SendCallback(IAsyncResult ar)
            {
                Socket socket = ((SystemNetSocket)ar.AsyncState).Socket;
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
                    ((SystemNetSocket)PSocket).BeginReceive(PReceiveState.Stream.GetBuffer(), (int)PReceiveState.Stream.Position, (int)(PReceiveState.Stream.Length - PReceiveState.Stream.Position), SocketFlags.None, _receiveCallback, PSocket);
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
                Socket socket = ((INetworkSocket)ar.AsyncState).Socket;
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
                if (PReceiveState.PacketHeaderHandler != null)
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
    }
}*/