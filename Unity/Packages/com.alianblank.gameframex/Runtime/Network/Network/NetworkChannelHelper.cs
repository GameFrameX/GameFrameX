using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using GameFrameX;
using GameFrameX.Event;
using GameFrameX.Network;
using GameFrameX.Runtime;


namespace GameMain
{
    public class DefaultNetworkChannelHelper : INetworkChannelHelper, IReference
    {
        private MemoryStream _cachedStream;
        private INetworkChannel _netChannel;

        public DefaultNetworkChannelHelper()
        {
            // _s2cPacketTypes = new Dictionary<int, Type>();
            _cachedStream = new MemoryStream(1024);
            _netChannel = null;
        }


        public void Initialize(INetworkChannel netChannel)
        {
            _netChannel = netChannel;
            // 反射注册包和包处理函数。
            var packetReceiveHeaderHandlerBaseType = typeof(IPacketReceiveHeaderHandler);
            var packetReceiveBodyHandlerBaseType = typeof(IPacketReceiveBodyHandler);
            var packetSendHeaderHandlerBaseType = typeof(IPacketSendHeaderHandler);
            var packetSendBodyHandlerBaseType = typeof(IPacketSendBodyHandler);
            var packetHandlerBaseType = typeof(IPacketHandler);
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                if (!type.IsImplWithInterface(packetHandlerBaseType))
                {
                    continue;
                }

                if (type.IsImplWithInterface(packetReceiveHeaderHandlerBaseType))
                {
                    var packetHandler = (IPacketReceiveHeaderHandler)Activator.CreateInstance(type);
                    _netChannel.RegisterHandler(packetHandler);
                }
                else if (type.IsImplWithInterface(packetReceiveBodyHandlerBaseType))
                {
                    var packetHandler = (IPacketReceiveBodyHandler)Activator.CreateInstance(type);
                    _netChannel.RegisterHandler(packetHandler);
                }
                else if (type.IsImplWithInterface(packetSendHeaderHandlerBaseType))
                {
                    var packetHandler = (IPacketSendHeaderHandler)Activator.CreateInstance(type);
                    _netChannel.RegisterHandler(packetHandler);
                }
                else if (type.IsImplWithInterface(packetSendBodyHandlerBaseType))
                {
                    var packetHandler = (IPacketSendBodyHandler)Activator.CreateInstance(type);
                    _netChannel.RegisterHandler(packetHandler);
                }
            }

            GameApp.Event.Subscribe(NetworkConnectedEventArgs.EventId, OnNetConnected);
            GameApp.Event.Subscribe(NetworkClosedEventArgs.EventId, OnNetClosed);
            GameApp.Event.Subscribe(NetworkMissHeartBeatEventArgs.EventId, OnNetMissHeartBeat);
            GameApp.Event.Subscribe(NetworkErrorEventArgs.EventId, OnNetError);
            GameApp.Event.Subscribe(NetworkConnectedEventArgs.EventId, OnNetCustomError);
        }

        public void Shutdown()
        {
            GameApp.Event.Unsubscribe(NetworkConnectedEventArgs.EventId, OnNetConnected);
            GameApp.Event.Unsubscribe(NetworkClosedEventArgs.EventId, OnNetClosed);
            GameApp.Event.Unsubscribe(NetworkMissHeartBeatEventArgs.EventId, OnNetMissHeartBeat);
            GameApp.Event.Unsubscribe(NetworkErrorEventArgs.EventId, OnNetError);
            GameApp.Event.Unsubscribe(NetworkConnectedEventArgs.EventId, OnNetCustomError);
            _netChannel = null;
        }

        public void PrepareForConnecting()
        {
            _netChannel.Socket.ReceiveBufferSize = 1024 * 1024 * 5;
            _netChannel.Socket.SendBufferSize = 1024 * 1024 * 5;
        }

        public bool SendHeartBeat()
        {
            //TODO
            Log.Info("SendHeartBeat");
            return true;
        }

        public bool SerializePacketHeader<T>(T messageObject, Stream destination, out byte[] messageBodyBuffer) where T : MessageObject
        {
            GameFrameworkGuard.NotNull(_netChannel, nameof(_netChannel));
            GameFrameworkGuard.NotNull(_netChannel.PacketSendHeaderHandler, nameof(_netChannel.PacketSendHeaderHandler));
            GameFrameworkGuard.NotNull(messageObject, nameof(messageObject));
            GameFrameworkGuard.NotNull(destination, nameof(destination));

            return _netChannel.PacketSendHeaderHandler.Handler(messageObject, _cachedStream, out messageBodyBuffer);
        }

        public bool SerializePacketBody(byte[] messageBodyBuffer, Stream destination)
        {
            GameFrameworkGuard.NotNull(_netChannel, nameof(_netChannel));
            GameFrameworkGuard.NotNull(_netChannel.PacketSendHeaderHandler, nameof(_netChannel.PacketSendHeaderHandler));
            GameFrameworkGuard.NotNull(_netChannel.PacketSendBodyHandler, nameof(_netChannel.PacketSendBodyHandler));
            GameFrameworkGuard.NotNull(messageBodyBuffer, nameof(messageBodyBuffer));
            GameFrameworkGuard.NotNull(destination, nameof(destination));

            return _netChannel.PacketSendBodyHandler.Handler(messageBodyBuffer, _cachedStream, destination);
        }

        public bool DeserializePacketHeader(object source)
        {
            GameFrameworkGuard.NotNull(source, nameof(source));

            return _netChannel.PacketReceiveHeaderHandler.Handler(source);
        }

        public bool DeserializePacketBody(object source, out MessageObject messageObject)
        {
            GameFrameworkGuard.NotNull(source, nameof(source));

            return _netChannel.PacketReceiveBodyHandler.Handler(source, out messageObject);
        }

        public void Clear()
        {
            _cachedStream = null;
            _netChannel = null;
        }

        private void OnNetConnected(object sender, GameEventArgs e)
        {
            var ne = e as NetworkConnectedEventArgs;
            if (ne == null || ne.NetworkChannel != _netChannel)
            {
                return;
            }

            Log.Info("网络连接成功......");
        }

        private void OnNetClosed(object sender, GameEventArgs e)
        {
            var ne = e as NetworkClosedEventArgs;
            if (ne == null || ne.NetworkChannel != _netChannel)
            {
                return;
            }

            Log.Info("网络连接关闭......");
        }

        private void OnNetMissHeartBeat(object sender, GameEventArgs e)
        {
            var ne = e as NetworkMissHeartBeatEventArgs;
            if (ne == null || ne.NetworkChannel != _netChannel) return;
            Log.Warning(Utility.Text.Format("Network channel '{0}' miss heart beat '{1}' times.", ne.NetworkChannel.Name, ne.MissCount));
            // if (ne.MissCount < 2) return;
            // ne.NetChannel.Close();
        }

        private void OnNetError(object sender, GameEventArgs e)
        {
            var ne = e as NetworkErrorEventArgs;
            if (ne == null || ne.NetworkChannel != _netChannel)
            {
                return;
            }

            Log.Error(Utility.Text.Format("Network channel '{0}' error, error code is '{1}', error message is '{2}'.", ne.NetworkChannel.Name, ne.ErrorCode, ne.ErrorMessage));
            //ne.NetworkChannel.Close();
        }

        private void OnNetCustomError(object sender, GameEventArgs e)
        {
            var ne = e as NetworkCustomErrorEventArgs;
            if (ne == null || ne.NetworkChannel != _netChannel)
            {
                return;
            }
        }
    }
}