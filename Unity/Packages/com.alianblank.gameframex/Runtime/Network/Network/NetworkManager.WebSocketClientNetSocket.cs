using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityWebSocket;

namespace GameFrameX.Network
{
    public partial class NetworkManager
    {
        private sealed class WebSocketClientNetSocket : INetworkSocket
        {
            private readonly IWebSocket _client;

            /// <summary>
            /// 是否是加密协议
            /// </summary>
            private readonly bool IsSSL = false;

            TaskCompletionSource<bool> _connectTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private Action<byte[]> _action;

            public WebSocketClientNetSocket(IPAddress ipAddress, int port, Action<byte[]> action)
            {
                _client = new WebSocket("ws://" + ipAddress + ":" + port + "/" + (IsSSL ? "wss" : "ws"));
                _action = action;
                _client.OnOpen += OnOpen;
                _client.OnError += OnError;
                _client.OnClose += OnClose;
                _client.OnMessage += OnMessage;
            }

            private void OnMessage(object sender, MessageEventArgs e)
            {
                if (e.IsBinary)
                {
                    _action.Invoke(e.RawData);
                }
            }

            private void OnClose(object sender, CloseEventArgs e)
            {
            }

            private void OnError(object sender, ErrorEventArgs e)
            {
                _connectTask.TrySetResult(false);
            }

            private void OnOpen(object sender, OpenEventArgs e)
            {
                _connectTask.TrySetResult(true);
            }


            public async Task ConnectAsync()
            {
                _connectTask = new TaskCompletionSource<bool>();
                _client.ConnectAsync();
                await _connectTask.Task;
            }

            public IWebSocket Client
            {
                get { return _client; }
            }

            public bool IsConnected
            {
                get { return _client.IsConnected; }
            }

            public EndPoint LocalEndPoint
            {
                get { return null; }
            }

            public EndPoint RemoteEndPoint
            {
                get { return null; }
            }

            public int ReceiveBufferSize { get; set; }
            public int SendBufferSize { get; set; }

            public void Shutdown()
            {
                _client.CloseAsync();
            }

            public void Close()
            {
                _client.CloseAsync();
            }
        }
    }
}