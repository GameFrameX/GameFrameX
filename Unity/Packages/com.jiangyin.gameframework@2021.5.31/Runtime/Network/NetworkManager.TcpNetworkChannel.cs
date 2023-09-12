//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Base.Net;
using Bedrock.Framework.Protocols;
using UnityEngine;

namespace GameFramework.Network
{
    public sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        /// <summary>
        /// TCP 网络频道。
        /// </summary>
        private sealed class TcpNetworkChannel : NetworkChannelBase
        {
            private readonly Action<ConnectState> m_ConnectCallback;
            private readonly AsyncCallback m_SendCallback;
            private readonly AsyncCallback m_ReceiveCallback;


            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public TcpNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
                m_ConnectCallback = ConnectCallback;
                m_SendCallback = SendCallback;
                m_ReceiveCallback = ReceiveCallback;
            }


            public void Init(Func<int, Type> getMsgTypeFunc)
            {
                // Protocol = new ClientProtocol(getMsgTypeFunc);
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="host">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public override void Connect(string host, int port, object userData)
            {
                base.Connect(host, port, userData);
                System.Net.Sockets.AddressFamily ipType;
                (ipType, host) = Utility.Net.GetIPv6Address(host, port);
                switch (ipType)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        m_AddressFamily = AddressFamily.IPv4;
                        break;

                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        m_AddressFamily = AddressFamily.IPv6;
                        break;
                }

                m_SocketConnection = new SocketConnection(ipType, host, port);
                if (m_SocketConnection == null)
                {
                    string errorMessage = "Initialize network channel failure.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.SocketError, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                // Init();
                m_NetworkChannelHelper.PrepareForConnecting();
                ConnectAsync();
            }

            public IProtoCal<Message> Protocol { get; protected set; }

            protected override bool ProcessSend()
            {
                if (base.ProcessSend())
                {
                    SendAsync();
                    return true;
                }

                return false;
            }

            private async void ConnectAsync()
            {
                try
                {
                    var context = await m_SocketConnection.StartAsync();
                    ConnectState socketUserData = new ConnectState(m_SocketConnection, m_userData);
                    if (context != null)
                    {
                        Reader = context.CreateReader();
                        Writer = context.CreateWriter();
                        // Protocol = protoCal;
                        // onMessageAct = onMessage;
                        context.ConnectionClosed.Register(ConnectionClosed);
                        m_ConnectCallback(socketUserData);
                    }
                    else
                    {
                        throw new SocketException();
                    }
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }


            private void ConnectCallback(ConnectState socketUserData)
            {
                try
                {
                    bool isConnected = socketUserData.SocketConnection.Socket.Connected;
                    if (!isConnected)
                    {
                        throw new SocketException();
                    }
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                m_SentPacketCount = 0;
                m_ReceivedPacketCount = 0;

                lock (m_SendPacketPool)
                {
                    m_SendPacketPool.Clear();
                }

                m_ReceivePacketPool.Clear();

                lock (m_HeartBeatState)
                {
                    m_HeartBeatState.Reset(true);
                }

                if (NetworkChannelConnected != null)
                {
                    NetworkChannelConnected(this, socketUserData.UserData);
                }

                m_Active = true;
                ReceiveAsync();
            }

            private void SendAsync()
            {
                try
                {
                    // m_Socket.BeginSend(m_SendState.Stream.GetBuffer(), (int) m_SendState.Stream.Position, (int) (m_SendState.Stream.Length - m_SendState.Stream.Position), SocketFlags.None, m_SendCallback, m_Socket);
                }
                catch (Exception exception)
                {
                    m_Active = false;
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
                Socket socket = (Socket) ar.AsyncState;
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
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                // m_SendState.Stream.Position += bytesSent;
                // if (m_SendState.Stream.Position < m_SendState.Stream.Length)
                // {
                //     SendAsync();
                //     return;
                // }

                m_SentPacketCount++;
                // m_SendState.Reset();
            }

            public async Task StartReadMsgAsync()
            {
                while (Reader != null && Writer != null)
                {
                    try
                    {
                        var result = await Reader.ReadAsync(Protocol);

                        var message = result.Message;

                        // onMessageAct(message);

                        if (result.IsCompleted)
                            break;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        break;
                    }
                }
            }

            public void Write(Message msg)
            {
                // _ = Writer?.WriteAsync(Protocol, msg);
                m_SentPacketCount++;
            }


            void ConnectionClosed()
            {
                NetworkChannelClosed?.Invoke(this);
                Reader = null;
                Writer = null;
            }

            public bool IsClose()
            {
                return Reader == null || Writer == null;
            }

            private void ReceiveAsync()
            {
                try
                {
                    // m_Socket.BeginReceive(m_ReceiveState.Stream.GetBuffer(), (int) m_ReceiveState.Stream.Position, (int) (m_ReceiveState.Stream.Length - m_ReceiveState.Stream.Position), SocketFlags.None, m_ReceiveCallback, m_Socket);
                }
                catch (Exception exception)
                {
                    m_Active = false;
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
                Socket socket = (Socket) ar.AsyncState;
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
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    // throw;
                    return;
                }

                if (bytesReceived <= 0)
                {
                    Close();
                    return;
                }

                // m_ReceiveState.Stream.Position += bytesReceived;
                // if (m_ReceiveState.Stream.Position < m_ReceiveState.Stream.Length)
                // {
                //     ReceiveAsync();
                //     return;
                // }
                //
                // m_ReceiveState.Stream.Position = 0L;
                //
                bool processSuccess = false;
                // if (m_ReceiveState.PacketHeader != null)
                // {
                //     processSuccess = ProcessPacket();
                //     m_ReceivedPacketCount++;
                // }
                // else
                // {
                //     processSuccess = ProcessPacketHeader();
                // }

                if (processSuccess)
                {
                    ReceiveAsync();
                    return;
                }
            }
        }
    }
}