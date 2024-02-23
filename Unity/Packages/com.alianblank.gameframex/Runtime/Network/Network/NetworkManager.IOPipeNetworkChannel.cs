//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameFrameX.Runtime;

namespace GameFrameX.Network
{
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// TCP 网络频道。
        /// </summary>
        private sealed class IOPipeNetworkChannel : NetworkChannelBase
        {
            private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
            private Pipe _receivePipe;

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public IOPipeNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
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
                PSocket = new IOPipeTcpClientNetSocket(ipAddress.AddressFamily);
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
                ConnectAsync(ipAddress, port, userData);
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

                bool serializeResult = base.ProcessSendMessage(messageObject); // PNetworkChannelHelper.SerializePacketHeader(messageObject, PSendState.Stream, out var messageBodyBuffer);
                if (serializeResult)
                {
                    var tcpClientNetSocket = (IOPipeTcpClientNetSocket)PSocket;
                    tcpClientNetSocket.Client.GetStream().Write(PSendState.Stream.GetBuffer(), 0, (int)PSendState.Stream.Position);
                }
                else
                {
                    const string errorMessage = "Serialized packet failure.";
                    throw new InvalidOperationException(errorMessage);
                }

                return true;
            }

            private async void ConnectAsync(IPAddress ipAddress, int port, object userData)
            {
                try
                {
                    await ((IOPipeTcpClientNetSocket)PSocket).Client.ConnectAsync(ipAddress, port);
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
                    var socketUserData = (IOPipeTcpClientNetSocket)PSocket;
                    if (!socketUserData.Client.Connected)
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
                _receivePipe = new Pipe();
                ReceiveAsync();
                ReceiveCallback();
            }

            private async void ReceiveAsync()
            {
                try
                {
                    byte[] readBuffer = new byte[2048];
                    var dataPipeWriter = _receivePipe.Writer;
                    var cancelToken = _cancellationTokenSource.Token;
                    while (!cancelToken.IsCancellationRequested)
                    {
                        var tcpClientNetSocket = (IOPipeTcpClientNetSocket)PSocket;
                        var length = await tcpClientNetSocket.Client.GetStream().ReadAsync(readBuffer, 0, readBuffer.Length, cancelToken);
                        if (length > 0)
                        {
                            dataPipeWriter.Write(readBuffer.AsSpan().Slice(0, length));
                            var flushTask = dataPipeWriter.FlushAsync();
                            if (!flushTask.IsCompleted)
                            {
                                await flushTask.ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }


                    //((SystemNetSocket)PSocket).BeginReceive(PReceiveState.Stream.GetBuffer(), (int)PReceiveState.Stream.Position, (int)(PReceiveState.Stream.Length - PReceiveState.Stream.Position), SocketFlags.None, _receiveCallback, PSocket);
                }
                catch (Exception exception)
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private async void ReceiveCallback()
            {
                try
                {
                    var cancelToken = _cancellationTokenSource.Token;
                    while (!cancelToken.IsCancellationRequested)
                    {
                        var result = await _receivePipe.Reader.ReadAsync(cancelToken);
                        var buffer = result.Buffer;
                        if (buffer.Length > 0)
                        {
                            PReceiveState.Stream.Position = 0L;

                            // PNetworkChannelHelper.DeserializePacketHeader(headerReader, out var packetHeader);
                            // 这里处理接收数据和解析
                            while (ProcessReceiveMessage(ref buffer))
                            {
                            }

                            _receivePipe.Reader.AdvanceTo(buffer.Start, buffer.End);
                        }
                        else if (result.IsCanceled || result.IsCompleted)
                        {
                            break;
                        }
                    }

                    // bytesReceived = socket.EndReceive(ar);
                }
                catch (Exception exception)
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                // if (bytesReceived <= 0)
                // {
                //     Close();
                //     return;
                // }

                // PReceiveState.Stream.Position += bytesReceived;
                if (PReceiveState.Stream.Position < PReceiveState.Stream.Length)
                {
                    ReceiveAsync();
                    return;
                }

                PReceiveState.Stream.Position = 0L;

                bool processSuccess = false;
                if (PReceiveState.PacketHeaderHandler != null)
                {
                    processSuccess = ProcessPacket();
                    PReceivedPacketCount++;
                }
                else
                {
                    processSuccess = ProcessPacketHeader();
                }

                if (processSuccess)
                {
                    ReceiveAsync();
                    return;
                }
            }

            private bool ProcessReceiveMessage(ref ReadOnlySequence<byte> buffer)
            {
                try
                {
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
                        result = PNetworkChannelHelper.DeserializePacketBody(body,PacketReceiveHeaderHandler.Id, out var messageObject);
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