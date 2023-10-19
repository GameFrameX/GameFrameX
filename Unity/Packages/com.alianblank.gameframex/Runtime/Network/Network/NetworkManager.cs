//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace GameFrameX.Network
{
    /// <summary>
    /// 网络管理器。
    /// </summary>
    public sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private readonly Dictionary<string, NetworkChannelBase> m_NetworkChannels;

        private EventHandler<NetworkConnectedEventArgs> m_NetworkConnectedEventHandler;
        private EventHandler<NetworkClosedEventArgs> m_NetworkClosedEventHandler;
        private EventHandler<NetworkMissHeartBeatEventArgs> m_NetworkMissHeartBeatEventHandler;
        private EventHandler<NetworkErrorEventArgs> m_NetworkErrorEventHandler;
        private EventHandler<NetworkCustomErrorEventArgs> m_NetworkCustomErrorEventHandler;

        /// <summary>
        /// 初始化网络管理器的新实例。
        /// </summary>
        public NetworkManager()
        {
            m_NetworkChannels = new Dictionary<string, NetworkChannelBase>(StringComparer.Ordinal);
            m_NetworkConnectedEventHandler = null;
            m_NetworkClosedEventHandler = null;
            m_NetworkMissHeartBeatEventHandler = null;
            m_NetworkErrorEventHandler = null;
            m_NetworkCustomErrorEventHandler = null;
        }

        /// <summary>
        /// 获取网络频道数量。
        /// </summary>
        public int NetworkChannelCount => m_NetworkChannels.Count;

        /// <summary>
        /// 网络连接成功事件。
        /// </summary>
        public event EventHandler<NetworkConnectedEventArgs> NetworkConnected
        {
            add => m_NetworkConnectedEventHandler += value;
            remove => m_NetworkConnectedEventHandler -= value;
        }

        /// <summary>
        /// 网络连接关闭事件。
        /// </summary>
        public event EventHandler<NetworkClosedEventArgs> NetworkClosed
        {
            add => m_NetworkClosedEventHandler += value;
            remove => m_NetworkClosedEventHandler -= value;
        }

        /// <summary>
        /// 网络心跳包丢失事件。
        /// </summary>
        public event EventHandler<NetworkMissHeartBeatEventArgs> NetworkMissHeartBeat
        {
            add => m_NetworkMissHeartBeatEventHandler += value;
            remove => m_NetworkMissHeartBeatEventHandler -= value;
        }

        /// <summary>
        /// 网络错误事件。
        /// </summary>
        public event EventHandler<NetworkErrorEventArgs> NetworkError
        {
            add => m_NetworkErrorEventHandler += value;
            remove => m_NetworkErrorEventHandler -= value;
        }

        /// <summary>
        /// 用户自定义网络错误事件。
        /// </summary>
        public event EventHandler<NetworkCustomErrorEventArgs> NetworkCustomError
        {
            add => m_NetworkCustomErrorEventHandler += value;
            remove => m_NetworkCustomErrorEventHandler -= value;
        }

        /// <summary>
        /// 网络管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var networkChannel in m_NetworkChannels)
            {
                networkChannel.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理网络管理器。
        /// </summary>
        internal override void Shutdown()
        {
            foreach (var networkChannel in m_NetworkChannels)
            {
                NetworkChannelBase networkChannelBase = networkChannel.Value;
                networkChannelBase.NetworkChannelConnected -= OnNetworkChannelConnected;
                networkChannelBase.NetworkChannelClosed -= OnNetworkChannelClosed;
                networkChannelBase.NetworkChannelMissHeartBeat -= OnNetworkChannelMissHeartBeat;
                networkChannelBase.NetworkChannelError -= OnNetworkChannelError;
                networkChannelBase.NetworkChannelCustomError -= OnNetworkChannelCustomError;
                networkChannelBase.Shutdown();
            }

            m_NetworkChannels.Clear();
        }

        /// <summary>
        /// 检查是否存在网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <returns>是否存在网络频道。</returns>
        public bool HasNetworkChannel(string channelName)
        {
            return m_NetworkChannels.ContainsKey(channelName ?? string.Empty);
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <returns>要获取的网络频道。</returns>
        public INetworkChannel GetNetworkChannel(string channelName)
        {
            if (m_NetworkChannels.TryGetValue(channelName ?? string.Empty, out var networkChannel))
            {
                return networkChannel;
            }

            return null;
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <returns>所有网络频道。</returns>
        public INetworkChannel[] GetAllNetworkChannels()
        {
            var index = 0;
            var results = new INetworkChannel[m_NetworkChannels.Count];
            foreach (var networkChannel in m_NetworkChannels)
            {
                results[index++] = networkChannel.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <param name="results">所有网络频道。</param>
        public void GetAllNetworkChannels(List<INetworkChannel> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var networkChannel in m_NetworkChannels)
            {
                results.Add(networkChannel.Value);
            }
        }

        /// <summary>
        /// 创建网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <param name="networkChannelHelper">网络频道辅助器。</param>
        /// <returns>要创建的网络频道。</returns>
        public INetworkChannel CreateNetworkChannel(string channelName, INetworkChannelHelper networkChannelHelper)
        {
            if (networkChannelHelper == null)
            {
                throw new GameFrameworkException("Network channel helper is invalid.");
            }

            if (networkChannelHelper.PacketHeaderLength < 0)
            {
                throw new GameFrameworkException("Packet header length is invalid.");
            }

            if (HasNetworkChannel(channelName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Already exist network channel '{0}'.", channelName ?? string.Empty));
            }

            NetworkChannelBase networkChannel = new TcpNetworkChannel(channelName, networkChannelHelper);

            networkChannel.NetworkChannelConnected += OnNetworkChannelConnected;
            networkChannel.NetworkChannelClosed += OnNetworkChannelClosed;
            networkChannel.NetworkChannelMissHeartBeat += OnNetworkChannelMissHeartBeat;
            networkChannel.NetworkChannelError += OnNetworkChannelError;
            networkChannel.NetworkChannelCustomError += OnNetworkChannelCustomError;
            m_NetworkChannels.Add(channelName, networkChannel);
            return networkChannel;
        }

        /// <summary>
        /// 销毁网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <returns>是否销毁网络频道成功。</returns>
        public bool DestroyNetworkChannel(string channelName)
        {
            if (m_NetworkChannels.TryGetValue(channelName ?? string.Empty, out var networkChannel))
            {
                networkChannel.NetworkChannelConnected -= OnNetworkChannelConnected;
                networkChannel.NetworkChannelClosed -= OnNetworkChannelClosed;
                networkChannel.NetworkChannelMissHeartBeat -= OnNetworkChannelMissHeartBeat;
                networkChannel.NetworkChannelError -= OnNetworkChannelError;
                networkChannel.NetworkChannelCustomError -= OnNetworkChannelCustomError;
                networkChannel.Shutdown();
                if (channelName != null)
                {
                    return m_NetworkChannels.Remove(channelName);
                }
            }

            return false;
        }

        private void OnNetworkChannelConnected(NetworkChannelBase networkChannel, object userData)
        {
            if (m_NetworkConnectedEventHandler != null)
            {
                lock (m_NetworkConnectedEventHandler)
                {
                    NetworkConnectedEventArgs networkConnectedEventArgs = NetworkConnectedEventArgs.Create(networkChannel, userData);
                    m_NetworkConnectedEventHandler(this, networkConnectedEventArgs);
                    ReferencePool.Release(networkConnectedEventArgs);
                }
            }
        }

        private void OnNetworkChannelClosed(NetworkChannelBase networkChannel)
        {
            if (m_NetworkClosedEventHandler != null)
            {
                lock (m_NetworkClosedEventHandler)
                {
                    NetworkClosedEventArgs networkClosedEventArgs = NetworkClosedEventArgs.Create(networkChannel);
                    m_NetworkClosedEventHandler(this, networkClosedEventArgs);
                    ReferencePool.Release(networkClosedEventArgs);
                }
            }
        }

        private void OnNetworkChannelMissHeartBeat(NetworkChannelBase networkChannel, int missHeartBeatCount)
        {
            if (m_NetworkMissHeartBeatEventHandler != null)
            {
                lock (m_NetworkMissHeartBeatEventHandler)
                {
                    NetworkMissHeartBeatEventArgs networkMissHeartBeatEventArgs = NetworkMissHeartBeatEventArgs.Create(networkChannel, missHeartBeatCount);
                    m_NetworkMissHeartBeatEventHandler(this, networkMissHeartBeatEventArgs);
                    ReferencePool.Release(networkMissHeartBeatEventArgs);
                }
            }
        }

        private void OnNetworkChannelError(NetworkChannelBase networkChannel, NetworkErrorCode errorCode, SocketError socketErrorCode, string errorMessage)
        {
            if (m_NetworkErrorEventHandler != null)
            {
                lock (m_NetworkErrorEventHandler)
                {
                    NetworkErrorEventArgs networkErrorEventArgs = NetworkErrorEventArgs.Create(networkChannel, errorCode, socketErrorCode, errorMessage);
                    m_NetworkErrorEventHandler(this, networkErrorEventArgs);
                    ReferencePool.Release(networkErrorEventArgs);
                }
            }
        }

        private void OnNetworkChannelCustomError(NetworkChannelBase networkChannel, object customErrorData)
        {
            if (m_NetworkCustomErrorEventHandler != null)
            {
                lock (m_NetworkCustomErrorEventHandler)
                {
                    NetworkCustomErrorEventArgs networkCustomErrorEventArgs = NetworkCustomErrorEventArgs.Create(networkChannel, customErrorData);
                    m_NetworkCustomErrorEventHandler(this, networkCustomErrorEventArgs);
                    ReferencePool.Release(networkCustomErrorEventArgs);
                }
            }
        }
    }
}