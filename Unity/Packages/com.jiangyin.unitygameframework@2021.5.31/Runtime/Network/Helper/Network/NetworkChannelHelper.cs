//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;
using GameFramework.Network;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public class NetworkChannelHelper : INetworkChannelHelper
    {
        private readonly Dictionary<int, Type> m_ServerToClientPacketTypes = new Dictionary<int, Type>();
        private readonly MemoryStream m_CachedStream = new MemoryStream(1024 * 8);
        private INetworkChannel m_NetworkChannel = null;

        /// <summary>
        /// 获取消息包头长度。消息号4+时间戳8+数据长度4=16
        /// </summary>
        public int PacketHeaderLength => sizeof(int) + sizeof(long) + sizeof(int);

        /// <summary>
        /// 初始化网络频道辅助器。
        /// </summary>
        /// <param name="networkChannel">网络频道。</param>
        public void Initialize(INetworkChannel networkChannel)
        {
            m_NetworkChannel = networkChannel;
            // 反射注册包和包处理函数。
            Type packetBaseType = typeof(SCPacketBodyBase);
            Type packetHandlerBaseType = typeof(PacketHandlerBase);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (!types[i].IsClass || types[i].IsAbstract)
                {
                    continue;
                }

                if (types[i].BaseType == packetBaseType)
                {
                    PacketBase packetBase = (PacketBase) Activator.CreateInstance(types[i]);
                    Type packetType = GetServerToClientPacketType(packetBase.Id);
                    if (packetType != null)
                    {
                        Log.Warning("Already exist packet type '{0}', check '{1}' or '{2}'?.", packetBase.Id.ToString(), packetType.Name, packetBase.GetType().Name);
                        continue;
                    }

                    m_ServerToClientPacketTypes.Add(packetBase.Id, types[i]);
                }
                else if (types[i].BaseType == packetHandlerBaseType)
                {
                    IPacketHandler packetHandler = (IPacketHandler) Activator.CreateInstance(types[i]);
                    m_NetworkChannel.RegisterHandler(packetHandler);
                }
            }

            GameApp.Event.Subscribe(UnityGameFramework.Runtime.NetworkConnectedEventArgs.EventId, OnNetworkConnected);
            GameApp.Event.Subscribe(UnityGameFramework.Runtime.NetworkClosedEventArgs.EventId, OnNetworkClosed);
            GameApp.Event.Subscribe(UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs.EventId, OnNetworkMissHeartBeat);
            GameApp.Event.Subscribe(UnityGameFramework.Runtime.NetworkErrorEventArgs.EventId, OnNetworkError);
            GameApp.Event.Subscribe(UnityGameFramework.Runtime.NetworkCustomErrorEventArgs.EventId, OnNetworkCustomError);
        }

        /// <summary>
        /// 关闭并清理网络频道辅助器。
        /// </summary>
        public void Shutdown()
        {
            GameApp.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkConnectedEventArgs.EventId, OnNetworkConnected);
            GameApp.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkClosedEventArgs.EventId, OnNetworkClosed);
            GameApp.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs.EventId, OnNetworkMissHeartBeat);
            GameApp.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkErrorEventArgs.EventId, OnNetworkError);
            GameApp.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkCustomErrorEventArgs.EventId, OnNetworkCustomError);

            m_NetworkChannel = null;
            m_CachedStream.Dispose();
        }

        /// <summary>
        /// 准备进行连接。
        /// </summary>
        public void PrepareForConnecting()
        {
            // m_NetworkChannel.SocketConnection.Socket.ReceiveBufferSize = 1024 * 64;
            // m_NetworkChannel.SocketConnection.Socket.SendBufferSize = 1024 * 64;
        }

        /// <summary>
        /// 发送心跳消息包。
        /// </summary>
        /// <returns>是否发送心跳消息包成功。</returns>
        public bool SendHeartBeat()
        {
            var req = ReferencePool.Acquire<CSMessagePackage>();
            var data = ReferencePool.Acquire<CSHeartBeat>();
            data.Timestamp = GameTimeHelper.UnixTimeMilliseconds();
            req.Code = data.Id;
            req.MessageType = typeof(CSHeartBeat);
            // req.Data = ProtobufHelper.ToBytes(data);
            ReferencePool.Release(data);
            m_NetworkChannel.Send(req);
            return true;
        }

        /// <summary>
        /// 序列化消息包。
        /// </summary>
        /// <typeparam name="T">消息包类型。</typeparam>
        /// <param name="packet">要序列化的消息包。</param>
        /// <param name="destination">要序列化的目标流。</param>
        /// <returns>是否序列化成功。</returns>
        public bool Serialize<T>(T packet, Stream destination) where T : Packet
        {
            if (!(packet is PacketBase packetImpl))
            {
                Log.Warning("Packet is invalid.");
                return false;
            }

            if (packetImpl.PacketType != PacketType.ClientToServer)
            {
                Log.Warning("Send packet invalid.");
                return false;
            }
#if UNITY_EDITOR
            // if (packet is CSMessagePackage cs)
            // {
            //     using (MemoryStream desMemoryStream = new MemoryStream(cs.Data))
            //     {
            //         var deserializedData = Serializer.Deserialize(cs.MessageType, desMemoryStream);
            //         var json = Utility.Json.ToJson(deserializedData);
            //         Log.Info("网络CS:" + json);
            //     }
            // }
#endif
            m_CachedStream.SetLength(m_CachedStream.Capacity); // 此行防止 Array.Copy 的数据无法写入
            m_CachedStream.Position = 0L;


            if (packet is CSMessagePackage packetHeader)
            {
                m_CachedStream.Seek(0, SeekOrigin.Begin);

                //消息码
                var head = BitConverter.GetBytes(packetHeader.Code);
                m_CachedStream.Write(head, 0, head.Length);
                //时间戳
                var timestamp = BitConverter.GetBytes((long) 11111111);
                m_CachedStream.Write(timestamp, 0, timestamp.Length);
                //数据长度
                var dataSize = BitConverter.GetBytes(packetHeader.Data.Length);
                m_CachedStream.Write(dataSize, 0, dataSize.Length);

                // 数据
                m_CachedStream.Write(packetHeader.Data, 0, packetHeader.Data.Length);
                ReferencePool.Release(packet);
            }

            m_CachedStream.WriteTo(destination);
            return true;
        }


        /// <summary>
        /// 反序列化消息包头。
        /// </summary>
        /// <param name="source">要反序列化的来源流。</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns>反序列化后的消息包头。</returns>
        public IPacketHeader DeserializePacketHeader(Stream source, out object customErrorData)
        {
            // 注意：此函数并不在主线程调用！
            customErrorData = null;
            var scHeader = ReferencePool.Acquire<SCPacketHeaderBase>();
            if (source is MemoryStream memoryStream)
            {
                ByteBuffer byteBuffer = ByteBuffer.Allocate(memoryStream.GetBuffer());
                scHeader.Id = byteBuffer.ReadInt();
                var timestamp = byteBuffer.ReadLong();
                var packetSize = byteBuffer.ReadInt();
                scHeader.PacketLength = packetSize;
                return scHeader;
            }

            return null;

            // return (IPacketHeader) RuntimeTypeModel.Default.Deserialize(source, scHeader, typeof(SCPacketHeaderBase));
        }

        /// <summary>
        /// 反序列化消息包。
        /// </summary>
        /// <param name="packetHeader">消息包头。</param>
        /// <param name="source">要反序列化的来源流。</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns>反序列化后的消息包。</returns>
        public Packet DeserializePacket(IPacketHeader packetHeader, Stream source, out object customErrorData)
        {
            // 注意：此函数并不在主线程调用！
            customErrorData = null;

            SCPacketHeaderBase scPacketHeader = packetHeader as SCPacketHeaderBase;
            if (scPacketHeader == null)
            {
                Log.Warning("Packet header is invalid.");
                return null;
            }

            Packet packet = null;
            if (scPacketHeader.IsValid)
            {
                Type packetType = GetServerToClientPacketType(scPacketHeader.Id);
                if (packetType != null)
                {
                    var type = ReferencePool.Acquire(packetType);

//                     var origin = RuntimeTypeModel.Default.Deserialize(source, type, packetType);
// #if UNITY_EDITOR
//                     var json = Utility.Json.ToJson(origin);
//                     Log.Info("网络SC:" + json);
// #endif

                    // packet = (Packet) origin;
                }
                else
                {
                    Log.Warning("Can not deserialize packet for packet id '{0}'.", scPacketHeader.Id.ToString());
                }
            }
            else
            {
                Log.Warning("Packet header is invalid.");
            }

            ReferencePool.Release(scPacketHeader);
            return packet;
        }

        private Type GetServerToClientPacketType(int id)
        {
            if (m_ServerToClientPacketTypes.TryGetValue(id, out var type))
            {
                return type;
            }

            return null;
        }

        private void OnNetworkConnected(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkConnectedEventArgs ne = (UnityGameFramework.Runtime.NetworkConnectedEventArgs) e;
            if (ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            // Log.Info("Network channel '{0}' connected, local address '{1}', remote address '{2}'.", ne.NetworkChannel.Name, ne.NetworkChannel.SocketConnection.LocalEndPoint.ToString(), ne.NetworkChannel.SocketConnection.RemoteEndPoint.ToString());
        }

        private void OnNetworkClosed(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkClosedEventArgs ne = (UnityGameFramework.Runtime.NetworkClosedEventArgs) e;
            if (ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Info("Network channel '{0}' closed.", ne.NetworkChannel.Name);
        }

        private void OnNetworkMissHeartBeat(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs ne = (UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs) e;
            if (ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Info("Network channel '{0}' miss heart beat '{1}' times.", ne.NetworkChannel.Name, ne.MissCount.ToString());

            if (ne.MissCount < 2)
            {
                return;
            }

            ne.NetworkChannel.Close();
        }

        private void OnNetworkError(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkErrorEventArgs ne = (UnityGameFramework.Runtime.NetworkErrorEventArgs) e;
            if (ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Info("Network channel '{0}' error, error code is '{1}', error message is '{2}'.", ne.NetworkChannel.Name, ne.ErrorCode.ToString(), ne.ErrorMessage);

            ne.NetworkChannel.Close();
        }

        private void OnNetworkCustomError(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkCustomErrorEventArgs ne = (UnityGameFramework.Runtime.NetworkCustomErrorEventArgs) e;
            if (ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }
        }
    }
}