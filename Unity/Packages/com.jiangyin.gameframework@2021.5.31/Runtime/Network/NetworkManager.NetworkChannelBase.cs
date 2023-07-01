//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Bedrock.Framework.Protocols;

namespace GameFramework.Network
{
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// 网络频道基类。
        /// </summary>
        private abstract class NetworkChannelBase : INetworkChannel, IDisposable
        {
            /// <summary>
            /// 默认心跳间隔时长,单位秒
            /// </summary>
            private const float DefaultHeartBeatInterval = 10f;

            private readonly string m_Name;

            /// <summary>
            /// 发送的消息包队列
            /// </summary>
            protected readonly Queue<Packet> m_SendPacketPool;

            /// <summary>
            /// 接收的消息包队列
            /// </summary>
            protected readonly EventPool<Packet> m_ReceivePacketPool;

            protected readonly INetworkChannelHelper m_NetworkChannelHelper;

            protected AddressFamily m_AddressFamily;
            protected bool m_ResetHeartBeatElapseSecondsWhenReceivePacket;

            /// <summary>
            /// 心跳间隔,单位秒
            /// </summary>
            protected float m_HeartBeatInterval;

            protected SocketConnection m_SocketConnection;

            // protected readonly SendState m_SendState;
            // protected readonly ReceiveState m_ReceiveState;

            /// <summary>
            /// 发送消息对象
            /// </summary>
            public ProtocolWriter Writer { get; protected set; }

            /// <summary>
            /// 接收消息对象
            /// </summary>
            public ProtocolReader Reader { get; protected set; }

            /// <summary>
            /// 心跳状态信息
            /// </summary>
            protected readonly HeartBeatState m_HeartBeatState;

            /// <summary>
            /// 累计发送的数据包数量
            /// </summary>
            protected int m_SentPacketCount;

            /// <summary>
            /// 累计接收的数据包数量
            /// </summary>
            protected int m_ReceivedPacketCount;

            /// <summary>
            /// 网络是否处于激活状态
            /// </summary>
            protected bool m_Active;

            /// <summary>
            /// 网络频道是否已销毁
            /// </summary>
            private bool m_Disposed;

            /// <summary>
            /// 用户自定义数据
            /// </summary>
            protected object m_userData;

            public Action<NetworkChannelBase, object> NetworkChannelConnected;
            public Action<NetworkChannelBase> NetworkChannelClosed;
            public Action<NetworkChannelBase, int> NetworkChannelMissHeartBeat;
            public Action<NetworkChannelBase, NetworkErrorCode, SocketError, string> NetworkChannelError;
            public Action<NetworkChannelBase, object> NetworkChannelCustomError;

            /// <summary>
            /// 初始化网络频道基类的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public NetworkChannelBase(string name, INetworkChannelHelper networkChannelHelper)
            {
                m_Name = name ?? string.Empty;
                m_SendPacketPool = new Queue<Packet>();
                m_ReceivePacketPool = new EventPool<Packet>(EventPoolMode.Default);
                m_NetworkChannelHelper = networkChannelHelper;
                m_AddressFamily = AddressFamily.Unknown;
                m_ResetHeartBeatElapseSecondsWhenReceivePacket = false;
                m_HeartBeatInterval = DefaultHeartBeatInterval;
                m_SocketConnection = null;
                // m_SendState = new SendState();
                // m_ReceiveState = new ReceiveState();
                m_HeartBeatState = new HeartBeatState();
                m_SentPacketCount = 0;
                m_ReceivedPacketCount = 0;
                m_Active = false;
                m_Disposed = false;

                NetworkChannelConnected = null;
                NetworkChannelClosed = null;
                NetworkChannelMissHeartBeat = null;
                NetworkChannelError = null;
                NetworkChannelCustomError = null;

                networkChannelHelper.Initialize(this);
            }

            /// <summary>
            /// 获取网络频道名称。
            /// </summary>
            public string Name => m_Name;

            /// <summary>
            /// 获取网络频道所使用的 Socket。
            /// </summary>
            public SocketConnection SocketConnection => m_SocketConnection;

            /// <summary>
            /// 获取是否已连接。
            /// </summary>
            public bool Connected
            {
                get
                {
                    if (m_SocketConnection != null)
                    {
                        return m_SocketConnection.Socket.Connected;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 获取网络地址类型。
            /// </summary>
            public AddressFamily AddressFamily => m_AddressFamily;

            /// <summary>
            /// 获取要发送的消息包数量。
            /// </summary>
            public int SendPacketCount => m_SendPacketPool.Count;

            /// <summary>
            /// 获取累计发送的消息包数量。
            /// </summary>
            public int SentPacketCount => m_SentPacketCount;

            /// <summary>
            /// 获取已接收未处理的消息包数量。
            /// </summary>
            public int ReceivePacketCount => m_ReceivePacketPool.EventCount;

            /// <summary>
            /// 获取累计已接收的消息包数量。
            /// </summary>
            public int ReceivedPacketCount => m_ReceivedPacketCount;

            /// <summary>
            /// 获取或设置当收到消息包时是否重置心跳流逝时间。
            /// </summary>
            public bool ResetHeartBeatElapseSecondsWhenReceivePacket
            {
                get => m_ResetHeartBeatElapseSecondsWhenReceivePacket;
                set => m_ResetHeartBeatElapseSecondsWhenReceivePacket = value;
            }

            /// <summary>
            /// 获取丢失心跳的次数。
            /// </summary>
            public int MissHeartBeatCount => m_HeartBeatState.MissHeartBeatCount;

            /// <summary>
            /// 获取或设置心跳间隔时长，以秒为单位。
            /// </summary>
            public float HeartBeatInterval
            {
                get => m_HeartBeatInterval;
                set => m_HeartBeatInterval = value;
            }

            /// <summary>
            /// 获取心跳等待时长，以秒为单位。
            /// </summary>
            public float HeartBeatElapseSeconds => m_HeartBeatState.HeartBeatElapseSeconds;

            /// <summary>
            /// 网络频道轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public virtual void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (m_SocketConnection == null || !m_Active)
                {
                    return;
                }

                ProcessSend();
                ProcessReceive();
                if (m_SocketConnection == null || !m_Active)
                {
                    return;
                }

                m_ReceivePacketPool.Update(elapseSeconds, realElapseSeconds);

                if (m_HeartBeatInterval > 0f)
                {
                    HeartBeatHandler(realElapseSeconds);
                }
            }

            /// <summary>
            /// 处理心跳信息
            /// </summary>
            /// <param name="realElapseSeconds"></param>
            private void HeartBeatHandler(float realElapseSeconds)
            {
                bool sendHeartBeat = false;
                int missHeartBeatCount = 0;
                lock (m_HeartBeatState)
                {
                    if (m_SocketConnection == null || !m_Active)
                    {
                        return;
                    }

                    m_HeartBeatState.HeartBeatElapseSeconds += realElapseSeconds;
                    if (m_HeartBeatState.HeartBeatElapseSeconds >= m_HeartBeatInterval)
                    {
                        sendHeartBeat = true;
                        missHeartBeatCount = m_HeartBeatState.MissHeartBeatCount;
                        m_HeartBeatState.HeartBeatElapseSeconds = 0f;
                        m_HeartBeatState.MissHeartBeatCount++;
                    }
                }

                if (sendHeartBeat && m_NetworkChannelHelper.SendHeartBeat())
                {
                    if (missHeartBeatCount > 0 && NetworkChannelMissHeartBeat != null)
                    {
                        NetworkChannelMissHeartBeat(this, missHeartBeatCount);
                    }
                }
            }

            /// <summary>
            /// 关闭网络频道。
            /// </summary>
            public virtual void Shutdown()
            {
                Close();
                m_ReceivePacketPool.Shutdown();
                m_NetworkChannelHelper.Shutdown();
            }

            /// <summary>
            /// 注册网络消息包处理函数。
            /// </summary>
            /// <param name="handler">要注册的网络消息包处理函数。</param>
            public void RegisterHandler(IPacketHandler handler)
            {
                if (handler == null)
                {
                    throw new GameFrameworkException("Packet handler is invalid.");
                }

                m_ReceivePacketPool.Subscribe(handler.Id, handler.Handle);
            }

            /// <summary>
            /// 设置默认事件处理函数。
            /// </summary>
            /// <param name="handler">要设置的默认事件处理函数。</param>
            public void SetDefaultHandler(EventHandler<Packet> handler)
            {
                m_ReceivePacketPool.SetDefaultHandler(handler);
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="host">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            public void Connect(string host, int port)
            {
                Connect(host, port, null);
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="host">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public virtual void Connect(string host, int port, object userData)
            {
                m_userData = userData;
                if (m_SocketConnection != null)
                {
                    Close();
                    m_SocketConnection = null;
                }

                // switch (ipAddress.AddressFamily)
                // {
                //     case System.Net.Sockets.AddressFamily.InterNetwork:
                //         m_AddressFamily = AddressFamily.IPv4;
                //         break;
                //
                //     case System.Net.Sockets.AddressFamily.InterNetworkV6:
                //         m_AddressFamily = AddressFamily.IPv6;
                //         break;
                //
                //     default:
                //         string errorMessage = Utility.Text.Format("Not supported address family '{0}'.", ipAddress.AddressFamily);
                //         if (NetworkChannelError != null)
                //         {
                //             NetworkChannelError(this, NetworkErrorCode.AddressFamilyError, SocketError.Success, errorMessage);
                //             return;
                //         }
                //
                //         throw new GameFrameworkException(errorMessage);
                // }

                // m_SendState.Reset();
                // m_ReceiveState.PrepareForPacketHeader(m_NetworkChannelHelper.PacketHeaderLength);
            }

            /// <summary>
            /// 关闭连接并释放所有相关资源。
            /// </summary>
            public void Close()
            {
                lock (this)
                {
                    if (m_SocketConnection == null)
                    {
                        return;
                    }

                    m_Active = false;

                    try
                    {
                        m_SocketConnection.Abort();
                        Reader.DisposeAsync();
                        Reader = null;
                        Writer.DisposeAsync().ConfigureAwait(false);
                        Writer = null;
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        m_SocketConnection.Socket.Close();
                        m_SocketConnection = null;

                        NetworkChannelClosed?.Invoke(this);
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
                }
            }

            /// <summary>
            /// 向远程主机发送消息包。
            /// </summary>
            /// <typeparam name="T">消息包类型。</typeparam>
            /// <param name="packet">要发送的消息包。</param>
            public void Send<T>(T packet) where T : Packet
            {
                if (m_SocketConnection == null)
                {
                    string errorMessage = "You must connect first.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (!m_Active)
                {
                    string errorMessage = "Socket is not active.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (packet == null)
                {
                    string errorMessage = "Packet is invalid.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                lock (m_SendPacketPool)
                {
                    m_SendPacketPool.Enqueue(packet);
                }
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            /// <param name="disposing">释放资源标记。</param>
            private void Dispose(bool disposing)
            {
                if (m_Disposed)
                {
                    return;
                }

                if (disposing)
                {
                    Close();
                    // m_SendState.Dispose();
                    // m_ReceiveState.Dispose();
                }

                m_Disposed = true;
            }

            protected virtual bool ProcessSend()
            {
                // if (m_SendState.Stream.Length > 0 || m_SendPacketPool.Count <= 0)
                if (m_SendPacketPool.Count <= 0)
                {
                    return false;
                }

                lock (m_SendPacketPool)
                {
                    while (m_SendPacketPool.Count > 0)
                    {
                        Packet packet = null;
                        lock (m_SendPacketPool)
                        {
                            packet = m_SendPacketPool.Dequeue();
                        }

                        bool serializeResult = false;
                        try
                        {
                            // Writer.WriteAsync()
                            // serializeResult = m_NetworkChannelHelper.Serialize(packet, m_SendState.Stream);
                        }
                        catch (Exception exception)
                        {
                            m_Active = false;
                            if (NetworkChannelError != null)
                            {
                                SocketException socketException = exception as SocketException;
                                NetworkChannelError(this, NetworkErrorCode.SerializeError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                                return false;
                            }

                            throw;
                        }

                        if (!serializeResult)
                        {
                            string errorMessage = "Serialized packet failure.";
                            if (NetworkChannelError != null)
                            {
                                NetworkChannelError(this, NetworkErrorCode.SerializeError, SocketError.Success, errorMessage);
                                return false;
                            }

                            throw new GameFrameworkException(errorMessage);
                        }
                    }
                }

                // m_SendState.Stream.Position = 0L;
                return true;
            }

            protected virtual void ProcessReceive()
            {
            }

            protected virtual bool ProcessPacketHeader()
            {
                try
                {
                    IPacketHeader packetHeader = null; //= m_NetworkChannelHelper.DeserializePacketHeader(m_ReceiveState.Stream, out var customErrorData);
                    object customErrorData = null;
                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    if (packetHeader == null)
                    {
                        string errorMessage = "Packet header is invalid.";
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, SocketError.Success, errorMessage);
                            return false;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }

                    // m_ReceiveState.PrepareForPacket(packetHeader);
                    if (packetHeader.PacketLength <= 0)
                    {
                        bool processSuccess = ProcessPacket();
                        m_ReceivedPacketCount++;
                        return processSuccess;
                    }
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }

            protected virtual bool ProcessPacket()
            {
                lock (m_HeartBeatState)
                {
                    m_HeartBeatState.Reset(m_ResetHeartBeatElapseSecondsWhenReceivePacket);
                }

                try
                {
                    Packet packet = null; //= m_NetworkChannelHelper.DeserializePacket(m_ReceiveState.PacketHeader, m_ReceiveState.Stream, out var customErrorData);
                    object customErrorData = null;
                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    if (packet != null)
                    {
                        m_ReceivePacketPool.Fire(this, packet);
                    }

                    // m_ReceiveState.PrepareForPacketHeader(m_NetworkChannelHelper.PacketHeaderLength);
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }
        }
    }
}