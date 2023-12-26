/*using System;
using System.Net;
using System.Net.Sockets;

namespace GameFrameX.Network
{
    public partial class NetworkManager
    {
        private sealed class SystemNetSocket : INetworkSocket
        {
            private readonly Socket _socket;


            public SystemNetSocket(System.Net.Sockets.AddressFamily ipAddressAddressFamily, SocketType socketType, ProtocolType protocolType)
            {
                _socket = new Socket(ipAddressAddressFamily, socketType, protocolType);
            }

            public bool IsConnected
            {
                get { return _socket.Connected; }
            }

            public Socket Socket
            {
                get { return _socket; }
            }

            public EndPoint LocalEndPoint
            {
                get { return _socket.LocalEndPoint; }
            }

            public EndPoint RemoteEndPoint
            {
                get { return _socket.RemoteEndPoint; }
            }

            public int Available
            {
                get { return _socket.Available; }
            }

            public int ReceiveBufferSize
            {
                get { return _socket.ReceiveBufferSize; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("Receive buffer size is invalid.", nameof(value));
                    }

                    _socket.ReceiveBufferSize = value;
                }
            }

            public int SendBufferSize
            {
                get { return _socket.SendBufferSize; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("Send buffer size is invalid.", nameof(value));
                    }

                    _socket.SendBufferSize = value;
                }
            }

            public void Shutdown()
            {
                _socket.Shutdown(SocketShutdown.Both);
            }

            public void Close()
            {
                _socket.Close();
            }


            public void BeginSend(byte[] getBuffer, int streamPosition, int streamLength, SocketFlags none, AsyncCallback mSendCallback, INetworkSocket mSocket)
            {
                _socket.BeginSend(getBuffer, streamPosition, streamLength, none, mSendCallback, mSocket);
            }

            public void BeginReceive(byte[] getBuffer, int streamPosition, int streamLength, SocketFlags none, AsyncCallback mReceiveCallback, INetworkSocket mSocket)
            {
                _socket.BeginReceive(getBuffer, streamPosition, streamLength, none, mReceiveCallback, mSocket);
            }

            public void BeginConnect(IPAddress ipAddress, int port, AsyncCallback mConnectCallback, ConnectState connectState)
            {
                _socket.BeginConnect(ipAddress, port, mConnectCallback, connectState);
            }

            public void EndConnect(IAsyncResult ar)
            {
                _socket.EndConnect(ar);
            }

            public int Receive(byte[] getBuffer, int streamPosition, int streamLength, SocketFlags none)
            {
                return _socket.Receive(getBuffer, streamPosition, streamLength, none);
            }
        }
    }
}*/