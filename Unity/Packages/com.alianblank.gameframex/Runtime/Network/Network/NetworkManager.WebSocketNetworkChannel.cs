//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using MessagePack;

namespace GameFrameX.Network
{
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// TCP 网络频道。
        /// </summary>
        private sealed class WebSocketNetworkChannel : NetworkChannelBase
        {
            private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public WebSocketNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
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
                PSocket = new WebSocketClientNetSocket(ipAddress, port, ReceiveCallback);
                if (PSocket == null)
                {
                    const string errorMessage = "Initialize network channel failure.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                PNetworkChannelHelper.PrepareForConnecting();
                ConnectAsync(userData);
            }

            public override void Close()
            {
                base.Close();
                _cancellationTokenSource.Cancel();
            }

            private bool IsClose()
            {
                return _cancellationTokenSource.IsCancellationRequested;
            }


            /// <summary>
            /// 处理发送消息对象
            /// </summary>
            /// <param name="messageObject">消息对象</param>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"></exception>
            protected override bool ProcessSendMessage(MessageObject messageObject)
            {
                if (IsClose())
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.Disconnecting, "Network channel is closing.");
                    }

                    return false;
                }

                bool serializeResult = base.ProcessSendMessage(messageObject);
                if (serializeResult)
                {
                    // TODO 效率不高
                    var tcpClientNetSocket = (WebSocketClientNetSocket)PSocket;
                    var messageBodyBuffer = PSendState.Stream.GetBuffer();
                    var input = new ReadOnlySequence<byte>(messageBodyBuffer, 0, (int)PSendState.Stream.Length);
                    var sendResult = input.ToArray();
                    tcpClientNetSocket.Client.SendAsync(sendResult);
                }
                else
                {
                    const string errorMessage = "Serialized packet failure.";
                    throw new InvalidOperationException(errorMessage);
                }

                return true;
            }

            private async void ConnectAsync(object userData)
            {
                try
                {
                    var socketClient = (WebSocketClientNetSocket)PSocket;
                    await socketClient.ConnectAsync();
                    ConnectCallback(new ConnectState(PSocket, userData));
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


            private void ConnectCallback(ConnectState connectState)
            {
                try
                {
                    var socketUserData = (WebSocketClientNetSocket)PSocket;
                    if (!socketUserData.IsConnected)
                    {
                        throw new SocketException((int)NetworkErrorCode.ConnectError);
                    }
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
                    NetworkChannelConnected(this, connectState.UserData);
                }

                PActive = true;
            }

            private async void ReceiveCallback(byte[] buffer)
            {
                // PReceiveState.Stream.Seek(0, SeekOrigin.Begin);
                // PReceiveState.Stream.Write(buffer, 0, buffer.Length);
                // if (PReceiveState.Stream.Position < PReceiveState.Stream.Length)
                // {
                //     return;
                // }

                // PReceiveState.Stream.Position = 0L;
                ReadOnlySequence<byte> bufferSequence = new ReadOnlySequence<byte>(buffer, 0, buffer.Length);

                ProcessReceiveMessage(ref bufferSequence);
                // bool processSuccess = false;
                // if (PReceiveState.PacketHeaderHandler != null)
                // {
                //     processSuccess = ProcessPacket();
                //     PReceivedPacketCount++;
                // }
                // else
                // {
                //     processSuccess = ProcessPacketHeader();
                // }
            }

            private bool ProcessReceiveMessage(ref ReadOnlySequence<byte> buffer)
            {
                try
                {
                    lock (PHeartBeatState)
                    {
                        PHeartBeatState.Reset(PResetHeartBeatElapseSecondsWhenReceivePacket);
                    }

                    PReceivedPacketCount++;

                    if (buffer.Length < PacketReceiveHeaderHandler.PacketHeaderLength)
                    {
                        return false;
                    }

                    var header = buffer.Slice(buffer.Start, PacketReceiveHeaderHandler.PacketHeaderLength);
                    buffer = buffer.Slice(PacketReceiveHeaderHandler.PacketHeaderLength);
                    var result = PNetworkChannelHelper.DeserializePacketHeader(header);
                    if (result)
                    {
                        var bodyLength = PacketReceiveHeaderHandler.PacketLength - PacketReceiveHeaderHandler.PacketHeaderLength;
                        if (buffer.Length < bodyLength)
                        {
                            return false;
                        }

                        var body = buffer.Slice(buffer.Start, bodyLength);
                        buffer = buffer.Slice(bodyLength);
                        result = PNetworkChannelHelper.DeserializePacketBody(body, out var messageObject);
#if UNITY_EDITOR
                        Log.Debug($"收到消息 ID:[{PacketReceiveHeaderHandler.Id}] ==>消息类型:{messageObject.GetType()} 消息内容:{Utility.Json.ToJson(messageObject)}");
#endif
                        if (!result)
                        {
                            if (NetworkChannelError != null)
                            {
                                NetworkChannelError(this, NetworkErrorCode.DeserializePacketError, SocketError.Success, "Packet body is invalid.");
                                return false;
                            }
                        }

                        PacketBase packetBase = ReferencePool.Acquire<PacketBase>();
                        packetBase.MessageObject = messageObject;
                        packetBase.MessageId = PacketReceiveHeaderHandler.Id;
                        PReceivePacketPool.Fire(this, packetBase);
                    }
                    else
                    {
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, SocketError.Success, "Packet header is invalid.");
                            return false;
                        }
                    }

                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}