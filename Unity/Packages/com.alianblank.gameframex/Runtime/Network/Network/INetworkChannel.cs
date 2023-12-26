//------------------------------------------------------------
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
    /// <summary>
    /// 网络频道接口。
    /// </summary>
    public interface INetworkChannel
    {
        /// <summary>
        /// 获取网络频道名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取网络频道所使用的 Socket。
        /// </summary>
        INetworkSocket Socket { get; }

        /// <summary>
        /// 获取是否已连接。
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 获取网络地址类型。
        /// </summary>
        AddressFamily AddressFamily { get; }

        /// <summary>
        /// 获取要发送的消息包数量。
        /// </summary>
        int SendPacketCount { get; }

        /// <summary>
        /// 获取累计发送的消息包数量。
        /// </summary>
        int SentPacketCount { get; }

        /// <summary>
        /// 获取已接收未处理的消息包数量。
        /// </summary>
        int ReceivePacketCount { get; }

        /// <summary>
        /// 获取累计已接收的消息包数量。
        /// </summary>
        int ReceivedPacketCount { get; }

        /// <summary>
        /// 获取或设置当收到消息包时是否重置心跳流逝时间。
        /// </summary>
        bool ResetHeartBeatElapseSecondsWhenReceivePacket { get; set; }

        /// <summary>
        /// 获取丢失心跳的次数。
        /// </summary>
        int MissHeartBeatCount { get; }

        /// <summary>
        /// 获取或设置心跳间隔时长，以秒为单位。
        /// </summary>
        float HeartBeatInterval { get; set; }

        /// <summary>
        /// 获取心跳等待时长，以秒为单位。
        /// </summary>
        float HeartBeatElapseSeconds { get; }

        /// <summary>
        /// 消息发送包头处理器
        /// </summary>
        IPacketSendHeaderHandler PacketSendHeaderHandler { get; }

        /// <summary>
        /// 消息发送内容处理器
        /// </summary>
        IPacketSendBodyHandler PacketSendBodyHandler { get; }

        /// <summary>
        /// 心跳消息处理器
        /// </summary>
        IPacketHeartBeatHandler PacketHeartBeatHandler { get; }

        /// <summary>
        /// 消息接收包头处理器
        /// </summary>
        IPacketReceiveHeaderHandler PacketReceiveHeaderHandler { get; }

        /// <summary>
        /// 消息接收内容处理器
        /// </summary>
        IPacketReceiveBodyHandler PacketReceiveBodyHandler { get; }

        /// <summary>
        /// 注册网络消息包处理函数。
        /// </summary>
        /// <param name="handler">要注册的网络消息包处理函数。</param>
        void RegisterHandler(IPacketSendHeaderHandler handler);

        /// <summary>
        /// 注册网络消息包处理函数。
        /// </summary>
        /// <param name="handler">要注册的网络消息包处理函数。</param>
        void RegisterHandler(IPacketSendBodyHandler handler);

        /// <summary>
        /// 注册网络消息包处理函数。
        /// </summary>
        /// <param name="handler">要注册的网络消息包处理函数。</param>
        void RegisterHandler(IPacketReceiveHeaderHandler handler);

        /// <summary>
        /// 注册网络消息包处理函数。
        /// </summary>
        /// <param name="handler">要注册的网络消息包处理函数。</param>
        void RegisterHandler(IPacketReceiveBodyHandler handler);

        /// <summary>
        /// 注册网络消息心跳处理函数，用于处理心跳消息
        /// </summary>
        /// <param name="handler">要注册的网络消息包处理函数</param>
        void RegisterHandler(IPacketHeartBeatHandler handler);

        /// <summary>
        /// 设置默认事件处理函数。
        /// </summary>
        /// <param name="handler">要设置的默认事件处理函数。</param>
        void SetDefaultHandler(EventHandler<Packet> handler);

        /// <summary>
        /// 连接到远程主机。
        /// </summary>
        /// <param name="ipAddress">远程主机的 IP 地址。</param>
        /// <param name="port">远程主机的端口号。</param>
        /// <param name="userData">用户自定义数据。</param>
        void Connect(IPAddress ipAddress, int port, object userData = null);

        /// <summary>
        /// 关闭网络频道。
        /// </summary>
        void Close();

        /// <summary>
        /// 向远程主机发送消息包。
        /// </summary>
        /// <typeparam name="T">消息包类型。</typeparam>
        /// <param name="messageObject">要发送的消息包。</param>
        void Send<T>(T messageObject) where T : MessageObject;
    }
}