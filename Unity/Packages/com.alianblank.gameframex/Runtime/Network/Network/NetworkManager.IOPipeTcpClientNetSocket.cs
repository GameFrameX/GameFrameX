using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace GameFrameX.Network
{
    public partial class NetworkManager
    {
        private sealed class IOPipeTcpClientNetSocket : INetworkSocket
        {
            private readonly TcpClient _client;


            public IOPipeTcpClientNetSocket(System.Net.Sockets.AddressFamily ipAddressAddressFamily)
            {
                _client = new TcpClient(ipAddressAddressFamily);
            }

            public bool IsConnected
            {
                get { return _client.Connected; }
            }

            public TcpClient Client
            {
                get { return _client; }
            }

            public Socket Socket
            {
                get { return _client.Client; }
            }

            public EndPoint LocalEndPoint
            {
                get { return _client.Client.LocalEndPoint; }
            }

            public EndPoint RemoteEndPoint
            {
                get { return _client.Client.RemoteEndPoint; }
            }

            public int Available
            {
                get { return _client.Available; }
            }

            public int ReceiveBufferSize
            {
                get { return _client.ReceiveBufferSize; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("Receive buffer size is invalid.", nameof(value));
                    }

                    _client.ReceiveBufferSize = value;
                }
            }

            public int SendBufferSize
            {
                get { return _client.SendBufferSize; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("Send buffer size is invalid.", nameof(value));
                    }

                    _client.SendBufferSize = value;
                }
            }

            public void Shutdown()
            {
                _client.Client.Shutdown(SocketShutdown.Both);
            }

            public void Close()
            {
                _client.Close();
            }

            //
            // public void BeginSend(byte[] getBuffer, int streamPosition, int streamLength, SocketFlags none, AsyncCallback mSendCallback, INetworkSocket mSocket)
            // {
            //     _tcpClient.BeginSend(getBuffer, streamPosition, streamLength, none, mSendCallback, mSocket);
            // }
            //
            // public void BeginReceive(byte[] getBuffer, int streamPosition, int streamLength, SocketFlags none, AsyncCallback mReceiveCallback, INetworkSocket mSocket)
            // {
            //     _socket.BeginReceive(getBuffer, streamPosition, streamLength, none, mReceiveCallback, mSocket);
            // }
            //
            // public void BeginConnect(IPAddress ipAddress, int port, AsyncCallback mConnectCallback, ConnectState connectState)
            // {
            //     _socket.BeginConnect(ipAddress, port, mConnectCallback, connectState);
            // }
            //
            // public void EndConnect(IAsyncResult ar)
            // {
            //     _socket.EndConnect(ar);
            // }
            //
            // public int Receive(byte[] getBuffer, int streamPosition, int streamLength, SocketFlags none)
            // {
            //     return _socket.Receive(getBuffer, streamPosition, streamLength, none);
            // }
        }
    }
}