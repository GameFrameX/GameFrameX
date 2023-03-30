using System;
using System.Collections.Generic;
using System.Net;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Event;
using GameFramework.Network;
using UnityEngine;

namespace UnityGameFramework.Runtime
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
        public EventHandler<S2CMessage> NotifyMessageHandler { get; set; }

        private LinkedList<Packet> packetList = new LinkedList<Packet>();
        private Dictionary<int, MessagePacket> messageList = new Dictionary<int, MessagePacket>();

        class MessagePacket
        {
            public int Index;
            public Packet Packet;
            public UniTaskCompletionSource<S2CMessage> Task;
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
            S2CMessage s2CMessage = (S2CMessage) e;
            if (messageList.TryGetValue(s2CMessage.index, out var messagePacket))
            {
                messagePacket.Task.TrySetResult(s2CMessage);
            }
            else
            {
                NotifyMessageHandler?.Invoke(sender, s2CMessage);
            }
        }

        private void OnConnected(object sender, GameEventArgs e)
        {
            NetworkConnectedEventArgs eventArgs = (NetworkConnectedEventArgs) e;
            if (eventArgs.NetworkChannel.Name.Equals(GameChannelName))
            {
                Log.Info("Connect Success ");
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

        public UniTask<S2CMessage> Call(C2SMessage packet)
        {
            UniTaskCompletionSource<S2CMessage> uniTask = new UniTaskCompletionSource<S2CMessage>();
            MessagePacket messagePacket = new MessagePacket();
            messagePacket.Index = ++Index;
            messagePacket.Task = uniTask;
            packet.index = messagePacket.Index;
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
                    NetworkChannel.Connect(IPAddress.Parse(host), port);
                }
            }
        }

        public void Reconnected()
        {
            NetworkChannelHelper = new Java_NetworkChannelHelper();
            NetworkChannel = Network.CreateNetworkChannel(GameChannelName, ServiceType.Tcp, NetworkChannelHelper);
            NetworkChannel.SetDefaultHandler(PacketHandler);
            NetworkChannel.Connect(IPAddress.Parse(toHost), toPort);
        }
    }
}