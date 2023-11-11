using System;
using System.Collections.Generic;
using System.Net;
using Cysharp.Threading.Tasks;
using GameFrameX;
using GameFrameX.Event;
using GameFrameX.Network;
using UnityEngine;

namespace GameFrameX.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/GameNetwork")]
    public class GameNetworkComponent : GameFrameworkComponent
    {
        public const string GameChannelName = "GameChannel";
        public NetworkComponent Network;
        public INetworkChannel NetworkChannel { get; private set; }
        private INetworkChannelHelper NetworkChannelHelper;
        private EventHandler<Packet> PacketHandler;

        /// <summary>
        /// 处理通知消息
        /// </summary>
        public EventHandler<Packet> NotifyMessageHandler { get; set; }

        private LinkedList<Packet> packetList = new LinkedList<Packet>();
        private Dictionary<int, MessagePacket> messageList = new Dictionary<int, MessagePacket>();

        class MessagePacket
        {
            public int Index;
            public Packet Packet;
            public UniTaskCompletionSource<Packet> Task;
        }

        public bool Connected { get; private set; }

        private int Index;

        protected override void Awake()
        {
            base.Awake();
            Index = 0;
        }

        private void Start()
        {
            GameEntry.GetComponent<EventComponent>().Subscribe(NetworkConnectedEventArgs.EventId, OnConnected);
            GameEntry.GetComponent<EventComponent>().Subscribe(NetworkClosedEventArgs.EventId, OnClosed);
            GameEntry.GetComponent<EventComponent>().Subscribe(NetworkErrorEventArgs.EventId, OnError);
            // GameEntry.GetComponent<EventComponent>().Subscribe(NetworkErrorEventArgs.EventId, OnError);
            // GameEntry.GetComponent<EventComponent>().Subscribe(NetworkConnectedEventArgs.EventId, OnConnected);
            Network = GameEntry.GetComponent<NetworkComponent>();
            PacketHandler = DefaultPacketHandler;
        }

        private void OnDestroy()
        {
            Network.DestroyNetworkChannel(GameChannelName);
        }

        private void OnError(object sender, GameEventArgs e)
        {
            Log.Error(e);
        }

        private void OnClosed(object sender, GameEventArgs e)
        {
            Network.DestroyNetworkChannel(GameChannelName);
            Connected = false;
            Log.Error("和游戏服务器断开链接");
            // GameEntry.GetComponent<EventComponent>().Fire(sender,);
            // GameApp.EventSystem.Run(EventIdType.UILoadingShow, UILoadingShowOption.Default());
        }

        private void DefaultPacketHandler(object sender, Packet e)
        {
            Packet s2CMessage = e;
            // if (messageList.TryGetValue(s2CMessage.Id, out var messagePacket))
            // {
            //     messagePacket.Task.TrySetResult(s2CMessage);
            // }
            // else
            // {
            //     NotifyMessageHandler?.Invoke(sender, s2CMessage);
            // }
        }

        private void OnConnected(object sender, GameEventArgs e)
        {
            NetworkConnectedEventArgs eventArgs = (NetworkConnectedEventArgs) e;
            if (eventArgs.NetworkChannel.Name.Equals(GameChannelName))
            {
                Log.Info("Connect Success ");
                NetworkChannelHelper.SendHeartBeat();
                Connected = true;
                if (packetList.Count > 0)
                {
                    while (packetList.Count > 0)
                    {
                        Send(packetList.First.Value);
                        packetList.RemoveFirst();
                    }
                }
            }
            else
            {
                Connected = false;
            }
        }

        public UniTask<Packet> Call(Packet packet)
        {
            UniTaskCompletionSource<Packet> uniTask = new UniTaskCompletionSource<Packet>();
            MessagePacket messagePacket = new MessagePacket();
            messagePacket.Index = ++Index;
            messagePacket.Task = uniTask;
            messageList[messagePacket.Index] = messagePacket;
            Send(packet);

            return uniTask.Task;
        }

        public void Send<T>(T packet) where T : Packet
        {
            if (Connected)
            {
                NetworkChannel?.Send(packet);
            }
            else
            {
                packetList.AddLast(packet);
            }
        }

        private string toHost;
        private int toPort;

        public void ConnectedToServer(string host, int port)
        {
            toHost = host;
            toPort = port;
            NetworkChannel = Network.GetNetworkChannel(GameChannelName);
            if (NetworkChannel == null)
            {
                Reconnected();
            }
            else
            {
                if (!NetworkChannel.Connected)
                {
                    // NetworkChannel.Connect(host, port);
                }
            }
        }

        public void Reconnected()
        {
            NetworkChannelHelper = new NetworkChannelHelper();
            NetworkChannel = Network.CreateNetworkChannel(GameChannelName, NetworkChannelHelper);
            NetworkChannel.SetDefaultHandler(PacketHandler);
            // NetworkChannel.Connect(toHost, toPort);
        }
    }
}