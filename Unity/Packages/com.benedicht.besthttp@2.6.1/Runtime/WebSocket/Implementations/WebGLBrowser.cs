#if (UNITY_WEBGL && !UNITY_EDITOR) && !BESTHTTP_DISABLE_WEBSOCKET
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.WebSocket
{
    delegate void OnWebGLWebSocketOpenDelegate(uint id);
    delegate void OnWebGLWebSocketTextDelegate(uint id, string text);
    delegate void OnWebGLWebSocketBinaryDelegate(uint id, IntPtr pBuffer, int length);
    delegate void OnWebGLWebSocketErrorDelegate(uint id, string error);
    delegate void OnWebGLWebSocketCloseDelegate(uint id, int code, string reason);

    internal sealed class WebGLBrowser : WebSocketBaseImplementation
    {
        public override WebSocketStates State => ImplementationId != 0 ? WS_GetState(ImplementationId) : WebSocketStates.Unknown;

        public override bool IsOpen => ImplementationId != 0 && WS_GetState(ImplementationId) == WebSocketStates.Open;
        public override int BufferedAmount => WS_GetBufferedAmount(ImplementationId);

        internal static Dictionary<uint, WebSocket> WebSockets = new Dictionary<uint, WebSocket>();

        private uint ImplementationId;

        public WebGLBrowser(WebSocket parent, Uri uri, string origin, string protocol) : base(parent, uri, origin, protocol)
        {
        }

        public override void StartOpen()
        {
            try
            {
                ImplementationId = WS_Create(this.Uri.OriginalString, this.Protocol, OnOpenCallback, OnTextCallback, OnBinaryCallback, OnErrorCallback, OnCloseCallback);
                WebSockets.Add(ImplementationId, this.Parent);
            }
            catch(Exception ex)
            {
                HTTPManager.Logger.Exception("WebSocket", "Open", ex, this.Parent.Context);
            }
        }

        public override void StartClose(UInt16 code, string message)
        {
            WS_Close(this.ImplementationId, code, message);
        }

        public override void Send(string message)
        {
            var count = System.Text.Encoding.UTF8.GetByteCount(message);
            var buffer = BufferPool.Get(count, true);

            System.Text.Encoding.UTF8.GetBytes(message, 0, message.Length, buffer, 0);

            WS_Send_String(this.ImplementationId, buffer, 0, count);

            BufferPool.Release(buffer);
        }

        public override void Send(byte[] buffer)
        {
            WS_Send_Binary(this.ImplementationId, buffer, 0, buffer.Length);
        }

        public override void Send(byte[] buffer, ulong offset, ulong count)
        {
            WS_Send_Binary(this.ImplementationId, buffer, (int)offset, (int)count);
        }

        [DllImport("__Internal")]
        static extern uint WS_Create(string url, string protocol, OnWebGLWebSocketOpenDelegate onOpen, OnWebGLWebSocketTextDelegate onText, OnWebGLWebSocketBinaryDelegate onBinary, OnWebGLWebSocketErrorDelegate onError, OnWebGLWebSocketCloseDelegate onClose);

        [DllImport("__Internal")]
        static extern WebSocketStates WS_GetState(uint id);
        
        [DllImport("__Internal")]
        static extern int WS_GetBufferedAmount(uint id);

        [DllImport("__Internal")]
        static extern int WS_Send_String(uint id, byte[] strData, int pos, int length);

        [DllImport("__Internal")]
        static extern int WS_Send_Binary(uint id, byte[] buffer, int pos, int length);

        [DllImport("__Internal")]
        static extern void WS_Close(uint id, ushort code, string reason);

        [DllImport("__Internal")]
        static extern void WS_Release(uint id);

        [AOT.MonoPInvokeCallback(typeof(OnWebGLWebSocketOpenDelegate))]
        static void OnOpenCallback(uint id)
        {
            WebSocket ws;
            if (WebSockets.TryGetValue(id, out ws))
            {
                if (ws.OnOpen != null)
                {
                    try
                    {
                        ws.OnOpen(ws);
                    }
                    catch(Exception ex)
                    {
                        HTTPManager.Logger.Exception("WebSocket", "OnOpen", ex, ws.Context);
                    }
                }
            }
            else
                HTTPManager.Logger.Warning("WebSocket", "OnOpenCallback - No WebSocket found for id: " + id.ToString(), ws.Context);
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLWebSocketTextDelegate))]
        static void OnTextCallback(uint id, string text)
        {
            WebSocket ws;
            if (WebSockets.TryGetValue(id, out ws))
            {
                if (ws.OnMessage != null)
                {
                    try
                    {
                        ws.OnMessage(ws, text);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("WebSocket", "OnMessage", ex, ws.Context);
                    }
                }
            }
            else
                HTTPManager.Logger.Warning("WebSocket", "OnTextCallback - No WebSocket found for id: " + id.ToString());
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLWebSocketBinaryDelegate))]
        static void OnBinaryCallback(uint id, IntPtr pBuffer, int length)
        {
            WebSocket ws;
            if (WebSockets.TryGetValue(id, out ws))
            {
                if (ws.OnBinary != null)
                {
                    try
                    {
                        byte[] buffer = new byte[length];

                        // Copy data from the 'unmanaged' memory to managed memory. Buffer will be reclaimed by the GC.
                        Marshal.Copy(pBuffer, buffer, 0, length);

                        ws.OnBinary(ws, buffer);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("WebSocket", "OnBinary", ex, ws.Context);
                    }
                }
            }
            else
                HTTPManager.Logger.Warning("WebSocket", "OnBinaryCallback - No WebSocket found for id: " + id.ToString());
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLWebSocketErrorDelegate))]
        static void OnErrorCallback(uint id, string error)
        {
            WebSocket ws;
            if (WebSockets.TryGetValue(id, out ws))
            {
                WebSockets.Remove(id);

                if (ws.OnError != null)
                {
                    try
                    {
                        ws.OnError(ws, error);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("WebSocket", "OnError", ex, ws.Context);
                    }
                }
            }
            else
                HTTPManager.Logger.Warning("WebSocket", "OnErrorCallback - No WebSocket found for id: " + id.ToString());

            try
            {
                WS_Release(id);
            }
            catch(Exception ex)
            {
                HTTPManager.Logger.Exception("WebSocket", "WS_Release", ex);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLWebSocketCloseDelegate))]
        static void OnCloseCallback(uint id, int code, string reason)
        {
            // To match non-webgl behavior, we have to treat this client-side generated message as an error
            if (code == (int)WebSocketStausCodes.ClosedAbnormally)
            {
                OnErrorCallback(id, "Abnormal disconnection.");
                return;
            }

            WebSocket ws;
            if (WebSockets.TryGetValue(id, out ws))
            {
                WebSockets.Remove(id);

                if (ws.OnClosed != null)
                {
                    try
                    {
                        ws.OnClosed(ws, (ushort)code, reason);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("WebSocket", "OnClosed", ex, ws.Context);
                    }
                }
            }
            else
                HTTPManager.Logger.Warning("WebSocket", "OnCloseCallback - No WebSocket found for id: " + id.ToString());

            try
            {
                WS_Release(id);
            }
            catch(Exception ex)
            {
                HTTPManager.Logger.Exception("WebSocket", "WS_Release", ex);
            }
        }
    }
}

#endif
