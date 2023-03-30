#if UNITY_WEBGL && !UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
#if !BESTHTTP_DISABLE_CACHING
using BestHTTP.Caching;
#endif
using BestHTTP.Core;
using BestHTTP.Extensions;
using BestHTTP.Connections;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.Connections
{
    delegate void OnWebGLRequestHandlerDelegate(int nativeId, int httpStatus, IntPtr pBuffer, int length, int zero);
    delegate void OnWebGLBufferDelegate(int nativeId, IntPtr pBuffer, int length);
    delegate void OnWebGLProgressDelegate(int nativeId, int downloaded, int total);
    delegate void OnWebGLErrorDelegate(int nativeId, string error);
    delegate void OnWebGLTimeoutDelegate(int nativeId);
    delegate void OnWebGLAbortedDelegate(int nativeId);

    internal sealed class WebGLConnection : ConnectionBase
    {
        static Dictionary<int, WebGLConnection> Connections = new Dictionary<int, WebGLConnection>(4);

        int NativeId;
        BufferSegmentStream Stream;

        public WebGLConnection(string serverAddress)
            : base(serverAddress, false)
        {
            XHR_SetLoglevel((byte)HTTPManager.Logger.Level);
        }

        public override void Shutdown(ShutdownTypes type)
        {
            base.Shutdown(type);

            XHR_Abort(this.NativeId);
        }

        protected override void ThreadFunc()
        {
            // XmlHttpRequest setup

            this.NativeId = XHR_Create(HTTPRequest.MethodNames[(byte)CurrentRequest.MethodType],
                                       CurrentRequest.CurrentUri.OriginalString,
                                       CurrentRequest.Credentials != null ? CurrentRequest.Credentials.UserName : null,
                                       CurrentRequest.Credentials != null ? CurrentRequest.Credentials.Password : null,
                                       CurrentRequest.WithCredentials ? 1 : 0);
            Connections.Add(NativeId, this);

            CurrentRequest.EnumerateHeaders((header, values) =>
                {
                    if (!header.Equals("Content-Length"))
                        for (int i = 0; i < values.Count; ++i)
                            XHR_SetRequestHeader(NativeId, header, values[i]);
                }, /*callBeforeSendCallback:*/ true);

            XHR_SetResponseHandler(NativeId, WebGLConnection.OnResponse, WebGLConnection.OnError, WebGLConnection.OnTimeout, WebGLConnection.OnAborted);
            // Setting OnUploadProgress result in an addEventListener("progress", ...) call making the request non-simple.
            // https://forum.unity.com/threads/best-http-released.200006/page-49#post-3696220
            XHR_SetProgressHandler(NativeId, 
                                   CurrentRequest.OnDownloadProgress == null ? (OnWebGLProgressDelegate)null : WebGLConnection.OnDownloadProgress, 
                                   CurrentRequest.OnUploadProgress == null ? (OnWebGLProgressDelegate)null : WebGLConnection.OnUploadProgress);

            XHR_SetTimeout(NativeId, (uint)(CurrentRequest.ConnectTimeout.TotalMilliseconds + CurrentRequest.Timeout.TotalMilliseconds));

            byte[] body = CurrentRequest.GetEntityBody();
            int length = 0;
            bool releaseBodyBuffer = false;

            if (body == null)
            {
                var upStreamInfo = CurrentRequest.GetUpStream();
                if (upStreamInfo.Stream != null)
                {
                    var internalBuffer = BufferPool.Get(upStreamInfo.Length > 0 ? upStreamInfo.Length : HTTPRequest.UploadChunkSize, true);
                    using (BufferPoolMemoryStream ms = new BufferPoolMemoryStream(internalBuffer, 0, internalBuffer.Length, true, true, false, true))
                    {
                        var buffer = BufferPool.Get(HTTPRequest.UploadChunkSize, true);
                        int readCount = -1;
                        while ((readCount = upStreamInfo.Stream.Read(buffer, 0, buffer.Length)) > 0)
                            ms.Write(buffer, 0, readCount);

                        BufferPool.Release(buffer);

                        length = (int)ms.Position;
                        body = ms.GetBuffer();

                        releaseBodyBuffer = true;
                    }
                }
            }
            else
            {
                length = body.Length;
            }

            XHR_Send(NativeId, body, length);

            if (releaseBodyBuffer)
                BufferPool.Release(body);
        }

#region Callback Implementations

        void OnResponse(int httpStatus, BufferSegment payload)
        {
            HTTPConnectionStates proposedConnectionState = HTTPConnectionStates.Processing;
            bool resendRequest = false;

            try
            {
                if (this.CurrentRequest.IsCancellationRequested)
                    return;

                using (var ms = new BufferSegmentStream())
                {
                    Stream = ms;

                    XHR_GetStatusLine(NativeId, OnBufferCallback);
                    XHR_GetResponseHeaders(NativeId, OnBufferCallback);

                    if (payload != BufferSegment.Empty)
                        ms.Write(payload);

                    SupportedProtocols protocol = HTTPProtocolFactory.GetProtocolFromUri(CurrentRequest.CurrentUri);
                    CurrentRequest.Response = HTTPProtocolFactory.Get(protocol, CurrentRequest, ms, CurrentRequest.UseStreaming, false);

                    CurrentRequest.Response.Receive(payload != BufferSegment.Empty && payload.Count > 0 ? (int)payload.Count : -1, true);

                    KeepAliveHeader keepAlive = null;
                    ConnectionHelper.HandleResponse(this.ToString(), this.CurrentRequest, out resendRequest, out proposedConnectionState, ref keepAlive);
                }
            }
            catch (Exception e)
            {
                HTTPManager.Logger.Exception(this.NativeId + " WebGLConnection", "OnResponse", e, this.Context);

                if (this.ShutdownType == ShutdownTypes.Immediate)
                    return;

#if !BESTHTTP_DISABLE_CACHING
                if (this.CurrentRequest.UseStreaming)
                    HTTPCacheService.DeleteEntity(this.CurrentRequest.CurrentUri);
#endif

                // Something gone bad, Response must be null!
                this.CurrentRequest.Response = null;

                if (!this.CurrentRequest.IsCancellationRequested)
                {
                    this.CurrentRequest.Exception = e;
                    this.CurrentRequest.State = HTTPRequestStates.Error;
                }

                proposedConnectionState = HTTPConnectionStates.Closed;
            }
            finally
            {
                // Exit ASAP
                if (this.ShutdownType != ShutdownTypes.Immediate)
                {
                    if (this.CurrentRequest.IsCancellationRequested)
                    {
                        // we don't know what stage the request is cancelled, we can't safely reuse the tcp channel.
                        proposedConnectionState = HTTPConnectionStates.Closed;

                        this.CurrentRequest.Response = null;

                        this.CurrentRequest.State = this.CurrentRequest.IsTimedOut ? HTTPRequestStates.TimedOut : HTTPRequestStates.Aborted;
                    }
                    else if (resendRequest)
                    {
                        RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(this.CurrentRequest, RequestEvents.Resend));
                    }
                    else if (this.CurrentRequest.Response != null && this.CurrentRequest.Response.IsUpgraded)
                    {
                        proposedConnectionState = HTTPConnectionStates.WaitForProtocolShutdown;
                    }
                    else if (this.CurrentRequest.State == HTTPRequestStates.Processing)
                    {
                        if (this.CurrentRequest.Response != null)
                            this.CurrentRequest.State = HTTPRequestStates.Finished;
                        else
                        {
                            this.CurrentRequest.Exception = new Exception(string.Format("[{0}] Remote server closed the connection before sending response header! Previous request state: {1}. Connection state: {2}",
                                    this.ToString(),
                                    this.CurrentRequest.State.ToString(),
                                    this.State.ToString()));
                            this.CurrentRequest.State = HTTPRequestStates.Error;

                            proposedConnectionState = HTTPConnectionStates.Closed;
                        }
                    }

                    this.CurrentRequest = null;

                    if (proposedConnectionState == HTTPConnectionStates.Processing)
                        proposedConnectionState = HTTPConnectionStates.Closed;

                    ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this, proposedConnectionState));
                }
            }
        }

        void OnBuffer(BufferSegment buffer)
        {
            if (Stream != null)
            {
                Stream.Write(buffer);
                //Stream.Write(HTTPRequest.EOL, 0, HTTPRequest.EOL.Length);
            }
        }

        void OnDownloadProgress(int down, int total)
        {
            RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(this.CurrentRequest, RequestEvents.DownloadProgress, down, total));
        }

        void OnUploadProgress(int up, int total)
        {
            RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(this.CurrentRequest, RequestEvents.UploadProgress, up, total));
        }

        void OnError(string error)
        {
            HTTPManager.Logger.Information(this.NativeId + " WebGLConnection - OnError", error, this.Context);

            LastProcessTime = DateTime.UtcNow;

            CurrentRequest.Response = null;
            CurrentRequest.Exception = new Exception(error);
            CurrentRequest.State = HTTPRequestStates.Error;
            ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this, HTTPConnectionStates.Closed));
        }

        void OnTimeout()
        {
            HTTPManager.Logger.Information(this.NativeId + " WebGLConnection - OnResponse", string.Empty, this.Context);

            CurrentRequest.Response = null;
            CurrentRequest.State = HTTPRequestStates.TimedOut;
            ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this, HTTPConnectionStates.Closed));
        }

        void OnAborted()
        {
            HTTPManager.Logger.Information(this.NativeId + " WebGLConnection - OnAborted", string.Empty, this.Context);

            CurrentRequest.Response = null;
            CurrentRequest.State = HTTPRequestStates.Aborted;
            ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this, HTTPConnectionStates.Closed));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Connections.Remove(NativeId);
                XHR_Release(NativeId);

                Stream = null;
            }
        }
#endregion

#region WebGL Static Callbacks

        [AOT.MonoPInvokeCallback(typeof(OnWebGLRequestHandlerDelegate))]
        static void OnResponse(int nativeId, int httpStatus, IntPtr pBuffer, int length, int err)
        {
            WebGLConnection conn = null;
            if (!Connections.TryGetValue(nativeId, out conn))
            {
                HTTPManager.Logger.Error("WebGLConnection - OnResponse", "No WebGL connection found for nativeId: " + nativeId.ToString());
                return;
            }

            HTTPManager.Logger.Information("WebGLConnection - OnResponse", string.Format("{0} {1} {2} {3}", nativeId, httpStatus, length, err), conn.Context);

            BufferSegment payload = BufferSegment.Empty;
            if (length > 0)
            {
                var buffer = BufferPool.Get(length, true);

                XHR_CopyResponseTo(nativeId, buffer, length);

                payload = new BufferSegment(buffer, 0, length);
            }

            conn.OnResponse(httpStatus, payload);
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLBufferDelegate))]
        static void OnBufferCallback(int nativeId, IntPtr pBuffer, int length)
        {
            WebGLConnection conn = null;
            if (!Connections.TryGetValue(nativeId, out conn))
            {
                HTTPManager.Logger.Error("WebGLConnection - OnBufferCallback", "No WebGL connection found for nativeId: " + nativeId.ToString());
                return;
            }

            byte[] buffer = BufferPool.Get(length, true);

            // Copy data from the 'unmanaged' memory to managed memory. Buffer will be reclaimed by the GC.
            Marshal.Copy(pBuffer, buffer, 0, length);

            conn.OnBuffer(new BufferSegment(buffer, 0, length));
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLProgressDelegate))]
        static void OnDownloadProgress(int nativeId, int downloaded, int total)
        {
            WebGLConnection conn = null;
            if (!Connections.TryGetValue(nativeId, out conn))
            {
                HTTPManager.Logger.Error("WebGLConnection - OnDownloadProgress", "No WebGL connection found for nativeId: " + nativeId.ToString());
                return;
            }

            HTTPManager.Logger.Information(nativeId + " OnDownloadProgress", downloaded.ToString() + " / " + total.ToString(), conn.Context);

            conn.OnDownloadProgress(downloaded, total);
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLProgressDelegate))]
        static void OnUploadProgress(int nativeId, int uploaded, int total)
        {
            WebGLConnection conn = null;
            if (!Connections.TryGetValue(nativeId, out conn))
            {
                HTTPManager.Logger.Error("WebGLConnection - OnUploadProgress", "No WebGL connection found for nativeId: " + nativeId.ToString());
                return;
            }

            HTTPManager.Logger.Information(nativeId + " OnUploadProgress", uploaded.ToString() + " / " + total.ToString(), conn.Context);

            conn.OnUploadProgress(uploaded, total);
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLErrorDelegate))]
        static void OnError(int nativeId, string error)
        {
            WebGLConnection conn = null;
            if (!Connections.TryGetValue(nativeId, out conn))
            {
                HTTPManager.Logger.Error("WebGLConnection - OnError", "No WebGL connection found for nativeId: " + nativeId.ToString() + " Error: " + error);
                return;
            }

            conn.OnError(error);
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLTimeoutDelegate))]
        static void OnTimeout(int nativeId)
        {
            WebGLConnection conn = null;
            if (!Connections.TryGetValue(nativeId, out conn))
            {
                HTTPManager.Logger.Error("WebGLConnection - OnTimeout", "No WebGL connection found for nativeId: " + nativeId.ToString());
                return;
            }

            conn.OnTimeout();
        }

        [AOT.MonoPInvokeCallback(typeof(OnWebGLAbortedDelegate))]
        static void OnAborted(int nativeId)
        {
            WebGLConnection conn = null;
            if (!Connections.TryGetValue(nativeId, out conn))
            {
                HTTPManager.Logger.Error("WebGLConnection - OnAborted", "No WebGL connection found for nativeId: " + nativeId.ToString());
                return;
            }

            conn.OnAborted();
        }

#endregion

#region WebGL Interface

        [DllImport("__Internal")]
        private static extern int XHR_Create(string method, string url, string userName, string passwd, int withCredentials);

        /// <summary>
        /// Is an unsigned long representing the number of milliseconds a request can take before automatically being terminated. A value of 0 (which is the default) means there is no timeout.
        /// </summary>
        [DllImport("__Internal")]
        private static extern void XHR_SetTimeout(int nativeId, uint timeout);

        [DllImport("__Internal")]
        private static extern void XHR_SetRequestHeader(int nativeId, string header, string value);

        [DllImport("__Internal")]
        private static extern void XHR_SetResponseHandler(int nativeId, OnWebGLRequestHandlerDelegate onresponse, OnWebGLErrorDelegate onerror, OnWebGLTimeoutDelegate ontimeout, OnWebGLAbortedDelegate onabort);

        [DllImport("__Internal")]
        private static extern void XHR_SetProgressHandler(int nativeId, OnWebGLProgressDelegate onDownloadProgress, OnWebGLProgressDelegate onUploadProgress);

        [DllImport("__Internal")]
        private static extern void XHR_CopyResponseTo(int nativeId, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 2)] byte[] response, int length);

        [DllImport("__Internal")]
        private static extern void XHR_Send(int nativeId, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 2)] byte[] body, int length);

        [DllImport("__Internal")]
        private static extern void XHR_GetResponseHeaders(int nativeId, OnWebGLBufferDelegate callback);

        [DllImport("__Internal")]
        private static extern void XHR_GetStatusLine(int nativeId, OnWebGLBufferDelegate callback);

        [DllImport("__Internal")]
        private static extern void XHR_Abort(int nativeId);

        [DllImport("__Internal")]
        private static extern void XHR_Release(int nativeId);

        [DllImport("__Internal")]
        private static extern void XHR_SetLoglevel(int logLevel);

#endregion
    }
}

#endif
