//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX;
using GameFrameX.Network;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 网络组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Network")]
    public sealed class NetworkComponent : GameFrameworkComponent
    {
        private INetworkManager _networkManager = null;
        private EventComponent _eventComponent = null;

        /// <summary>
        /// 获取网络频道数量。
        /// </summary>
        public int NetworkChannelCount => _networkManager.NetworkChannelCount;

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            new NetworkManager();
            _networkManager = GameFrameworkEntry.GetModule<INetworkManager>();
            if (_networkManager == null)
            {
                Log.Fatal("Network manager is invalid.");
                return;
            }

            _networkManager.NetworkConnected += OnNetworkConnected;
            _networkManager.NetworkClosed += OnNetworkClosed;
            _networkManager.NetworkMissHeartBeat += OnNetworkMissHeartBeat;
            _networkManager.NetworkError += OnNetworkError;
            _networkManager.NetworkCustomError += OnNetworkCustomError;
        }

        private void Start()
        {
            _eventComponent = GameEntry.GetComponent<EventComponent>();
            if (_eventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }
        }

        /// <summary>
        /// 检查是否存在网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <returns>是否存在网络频道。</returns>
        public bool HasNetworkChannel(string channelName)
        {
            return _networkManager.HasNetworkChannel(channelName);
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <returns>要获取的网络频道。</returns>
        public INetworkChannel GetNetworkChannel(string channelName)
        {
            return _networkManager.GetNetworkChannel(channelName);
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <returns>所有网络频道。</returns>
        public INetworkChannel[] GetAllNetworkChannels()
        {
            return _networkManager.GetAllNetworkChannels();
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <param name="results">所有网络频道。</param>
        public void GetAllNetworkChannels(List<INetworkChannel> results)
        {
            _networkManager.GetAllNetworkChannels(results);
        }

        /// <summary>
        /// 创建网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <param name="networkChannelHelper">网络频道辅助器。</param>
        /// <returns>要创建的网络频道。</returns>
        public INetworkChannel CreateNetworkChannel(string channelName, INetworkChannelHelper networkChannelHelper)
        {
            return _networkManager.CreateNetworkChannel(channelName, networkChannelHelper);
        }

        /// <summary>
        /// 销毁网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <returns>是否销毁网络频道成功。</returns>
        public bool DestroyNetworkChannel(string channelName)
        {
            return _networkManager.DestroyNetworkChannel(channelName);
        }

        private void OnNetworkConnected(object sender, NetworkConnectedEventArgs eventArgs)
        {
            _eventComponent.Fire(this, eventArgs);
        }

        private void OnNetworkClosed(object sender, NetworkClosedEventArgs eventArgs)
        {
            _eventComponent.Fire(this, eventArgs);
        }

        private void OnNetworkMissHeartBeat(object sender, NetworkMissHeartBeatEventArgs eventArgs)
        {
            _eventComponent.Fire(this, eventArgs);
        }

        private void OnNetworkError(object sender, NetworkErrorEventArgs eventArgs)
        {
            _eventComponent.Fire(this, eventArgs);
        }

        private void OnNetworkCustomError(object sender, NetworkCustomErrorEventArgs eventArgs)
        {
            _eventComponent.Fire(this, eventArgs);
        }
    }
}