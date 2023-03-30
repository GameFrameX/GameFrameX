#if !BESTHTTP_DISABLE_WEBSOCKET && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

using BestHTTP.Extensions;
using BestHTTP.WebSocket.Frames;
using BestHTTP.Core;
using BestHTTP.PlatformSupport.Memory;
using BestHTTP.Logger;

namespace BestHTTP.WebSocket
{
    public sealed class WebSocketResponse : HTTPResponse, IHeartbeat, IProtocol
    {
        /// <summary>
        /// Capacity of the RTT buffer where the latencies are kept.
        /// </summary>
        public static int RTTBufferCapacity = 5;

        #region Public Interface

        /// <summary>
        /// A reference to the original WebSocket instance. Used for accessing extensions.
        /// </summary>
        public WebSocket WebSocket { get; internal set; }

        /// <summary>
        /// Called when a Text message received
        /// </summary>
        public Action<WebSocketResponse, string> OnText;

        /// <summary>
        /// Called when a Binary message received
        /// </summary>
        public Action<WebSocketResponse, byte[]> OnBinary;

        /// <summary>
        /// Called when an incomplete frame received. No attempt will be made to reassemble these fragments.
        /// </summary>
        public Action<WebSocketResponse, WebSocketFrameReader> OnIncompleteFrame;

        /// <summary>
        /// Called when the connection closed.
        /// </summary>
        public Action<WebSocketResponse, UInt16, string> OnClosed;

        /// <summary>
        /// IProtocol's ConnectionKey property.
        /// </summary>
        public HostConnectionKey ConnectionKey { get; private set; }

        /// <summary>
        /// Indicates whether the connection to the server is closed or not.
        /// </summary>
        public bool IsClosed { get { return closed; } }

        /// <summary>
        /// IProtocol.LoggingContext implementation.
        /// </summary>
        LoggingContext IProtocol.LoggingContext { get => this.Context; }

        /// <summary>
        /// On what frequency we have to send a ping to the server.
        /// </summary>
        public TimeSpan PingFrequnecy { get; private set; }

        /// <summary>
        /// Maximum size of a fragment's payload data. Its default value is WebSocket.MaxFragmentSize's value.
        /// </summary>
        public uint MaxFragmentSize { get; set; }

        /// <summary>
        /// Length of unsent, buffered up data in bytes.
        /// </summary>
        public int BufferedAmount { get { return this._bufferedAmount; } }
        private int _bufferedAmount;

        /// <summary>
        /// Calculated latency from the Round-Trip Times we store in the rtts field.
        /// </summary>
        public int Latency { get; private set; }

        /// <summary>
        /// When we received the last frame.
        /// </summary>
        public DateTime lastMessage = DateTime.MinValue;

        #endregion

        #region Private Fields

        private List<WebSocketFrameReader> IncompleteFrames = new List<WebSocketFrameReader>();
        private ConcurrentQueue<WebSocketFrameReader> CompletedFrames = new ConcurrentQueue<WebSocketFrameReader>();
        private WebSocketFrameReader CloseFrame;

        private ConcurrentQueue<WebSocketFrame> unsentFrames = new ConcurrentQueue<WebSocketFrame>();
        private volatile AutoResetEvent newFrameSignal = new AutoResetEvent(false);
        private int sendThreadCreated = 0;
        private int closedThreads = 0;

        /// <summary>
        /// True if we sent out a Close message to the server
        /// </summary>
        private volatile bool closeSent;

        /// <summary>
        /// True if this WebSocket connection is closed
        /// </summary>
        private volatile bool closed;

        /// <summary>
        /// When we sent out the last ping.
        /// </summary>
        private DateTime lastPing = DateTime.MinValue;

        private bool waitingForPong = false;

        /// <summary>
        /// A circular buffer to store the last N rtt times calculated by the pong messages.
        /// </summary>
        private CircularBuffer<int> rtts = new CircularBuffer<int>(WebSocketResponse.RTTBufferCapacity);
        
        #endregion

        internal WebSocketResponse(HTTPRequest request, Stream stream, bool isStreamed, bool isFromCache)
            : base(request, stream, isStreamed, isFromCache)
        {
            base.IsClosedManually = true;
            this.ConnectionKey = new HostConnectionKey(this.baseRequest.CurrentUri.Host, HostDefinition.GetKeyForRequest(this.baseRequest));

            closed = false;
            MaxFragmentSize = WebSocket.MaxFragmentSize;
        }

        internal void StartReceive()
        {
            if (IsUpgraded)
                BestHTTP.PlatformSupport.Threading.ThreadedRunner.RunLongLiving(ReceiveThreadFunc);
        }

        internal void CloseStream()
        {
            if (base.Stream != null)
            {
                try
                {
                    base.Stream.Dispose();
                }
                catch
                { }
            }
        }

        #region Public interface for interacting with the server

        /// <summary>
        /// It will send the given message to the server in one frame.
        /// </summary>
        public void Send(string message)
        {
            if (message == null)
                throw new ArgumentNullException("message must not be null!");

            int count = System.Text.Encoding.UTF8.GetByteCount(message);
            byte[] data = BufferPool.Get(count, true);
            System.Text.Encoding.UTF8.GetBytes(message, 0, message.Length, data, 0);

            var frame = new WebSocketFrame(this.WebSocket, WebSocketFrameTypes.Text, data, 0, (ulong)count, true, true);

            if (frame.Data != null && frame.Data.Length > this.MaxFragmentSize)
            {
                WebSocketFrame[] additionalFrames = frame.Fragment(this.MaxFragmentSize);

                Send(frame);
                if (additionalFrames != null)
                    for (int i = 0; i < additionalFrames.Length; ++i)
                        Send(additionalFrames[i]);
            }
            else
                Send(frame);

            BufferPool.Release(data);
        }

        /// <summary>
        /// It will send the given data to the server in one frame.
        /// </summary>
        public void Send(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data must not be null!");

            WebSocketFrame frame = new WebSocketFrame(this.WebSocket, WebSocketFrameTypes.Binary, data);

            if (frame.Data != null && frame.Data.Length > this.MaxFragmentSize)
            {
                WebSocketFrame[] additionalFrames = frame.Fragment(this.MaxFragmentSize);

                Send(frame);
                if (additionalFrames != null)
                    for (int i = 0; i < additionalFrames.Length; ++i)
                        Send(additionalFrames[i]);
            }
            else
                Send(frame);
        }

        /// <summary>
        /// Will send count bytes from a byte array, starting from offset.
        /// </summary>
        public void Send(byte[] data, ulong offset, ulong count)
        {
            if (data == null)
                throw new ArgumentNullException("data must not be null!");
            if (offset + count > (ulong)data.Length)
                throw new ArgumentOutOfRangeException("offset + count >= data.Length");

            WebSocketFrame frame = new WebSocketFrame(this.WebSocket, WebSocketFrameTypes.Binary, data, offset, count, true, true);

            if (frame.Data != null && frame.Data.Length > this.MaxFragmentSize)
            {
                WebSocketFrame[] additionalFrames = frame.Fragment(this.MaxFragmentSize);

                Send(frame);

                if (additionalFrames != null)
                    for (int i = 0; i < additionalFrames.Length; ++i)
                        Send(additionalFrames[i]);
            }
            else
                Send(frame);
        }

        /// <summary>
        /// It will send the given frame to the server.
        /// </summary>
        public void Send(WebSocketFrame frame)
        {
            if (frame == null)
                throw new ArgumentNullException("frame is null!");

            if (closed || closeSent)
                return;

            this.unsentFrames.Enqueue(frame);

            if (Interlocked.CompareExchange(ref this.sendThreadCreated, 1, 0) == 0)
            {
                HTTPManager.Logger.Information("WebSocketResponse", "Send - Creating thread", this.Context);

                BestHTTP.PlatformSupport.Threading.ThreadedRunner.RunLongLiving(SendThreadFunc);
            }

            Interlocked.Add(ref this._bufferedAmount, frame.Data != null ? frame.DataLength : 0);

            //if (HTTPManager.Logger.Level <= Logger.Loglevels.All)
            //    HTTPManager.Logger.Information("WebSocketResponse", "Signaling SendThread!", this.Context);

            newFrameSignal.Set();
        }

        /// <summary>
        /// It will initiate the closing of the connection to the server.
        /// </summary>
        public void Close()
        {
            Close(1000, "Bye!");
        }

        /// <summary>
        /// It will initiate the closing of the connection to the server.
        /// </summary>
        public void Close(UInt16 code, string msg)
        {
            if (closed)
                return;

            HTTPManager.Logger.Verbose("WebSocketResponse", string.Format("Close({0}, \"{1}\")", code, msg), this.Context);

            WebSocketFrame frame;
            while (this.unsentFrames.TryDequeue(out frame))
                ;
            //this.unsentFrames.Clear();

            Interlocked.Exchange(ref this._bufferedAmount, 0);

            Send(new WebSocketFrame(this.WebSocket, WebSocketFrameTypes.ConnectionClose, WebSocket.EncodeCloseData(code, msg)));
        }

        public void StartPinging(int frequency)
        {
            if (frequency < 100)
                throw new ArgumentException("frequency must be at least 100 milliseconds!");

            PingFrequnecy = TimeSpan.FromMilliseconds(frequency);
            lastMessage = DateTime.UtcNow;

            SendPing();

            HTTPManager.Heartbeats.Subscribe(this);
            HTTPUpdateDelegator.OnApplicationForegroundStateChanged += OnApplicationForegroundStateChanged;
        }

        #endregion

        #region Private Threading Functions

        private void SendThreadFunc()
        {
            try
            {
                using (WriteOnlyBufferedStream bufferedStream = new WriteOnlyBufferedStream(this.Stream, 16 * 1024))
                {
                    while (!closed && !closeSent)
                    {
                        //if (HTTPManager.Logger.Level <= Logger.Loglevels.All)
                        //    HTTPManager.Logger.Information("WebSocketResponse", "SendThread - Waiting...", this.Context);
                        newFrameSignal.WaitOne();

                        try
                        {
                            //if (HTTPManager.Logger.Level <= Logger.Loglevels.All)
                            //    HTTPManager.Logger.Information("WebSocketResponse", "SendThread - Wait is over, about " + this.unsentFrames.Count.ToString() + " new frames!", this.Context);

                            WebSocketFrame frame;
                            while (this.unsentFrames.TryDequeue(out frame))
                            {
                                if (!closeSent)
                                {
                                    using (var rawData = frame.Get())
                                        bufferedStream.Write(rawData.Data, 0, rawData.Length);

                                    BufferPool.Release(frame.Data);

                                    if (frame.Type == WebSocketFrameTypes.ConnectionClose)
                                        closeSent = true;
                                }

                                Interlocked.Add(ref this._bufferedAmount, -frame.DataLength);
                            }

                            bufferedStream.Flush();
                        }
                        catch (Exception ex)
                        {
                            if (HTTPUpdateDelegator.IsCreated)
                            {
                                this.baseRequest.Exception = ex;
                                this.baseRequest.State = HTTPRequestStates.Error;
                            }
                            else
                                this.baseRequest.State = HTTPRequestStates.Aborted;

                            closed = true;
                        }
                    }

                    HTTPManager.Logger.Information("WebSocketResponse", string.Format("Ending Send thread. Closed: {0}, closeSent: {1}", closed, closeSent), this.Context);
                }
            }
            finally
            {
                Interlocked.Exchange(ref sendThreadCreated, 0);

                HTTPManager.Logger.Information("WebSocketResponse", "SendThread - Closed!", this.Context);

                TryToCleanup();
            }
        }

        private void ReceiveThreadFunc()
        {
            try
            {
                while (!closed)
                {
                    try
                    {
                        WebSocketFrameReader frame = new WebSocketFrameReader();
                        frame.Read(this.Stream);

                        if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                            HTTPManager.Logger.Information("WebSocketResponse", "Frame received: " + frame.Type, this.Context);

                        lastMessage = DateTime.UtcNow;

                        // A server MUST NOT mask any frames that it sends to the client.  A client MUST close a connection if it detects a masked frame.
                        // In this case, it MAY use the status code 1002 (protocol error)
                        // (These rules might be relaxed in a future specification.)
                        if (frame.HasMask)
                        {
                            HTTPManager.Logger.Warning("WebSocketResponse", "Protocol Error: masked frame received from server!", this.Context);
                            Close(1002, "Protocol Error: masked frame received from server!");
                            continue;
                        }

                        if (!frame.IsFinal)
                        {
                            if (OnIncompleteFrame == null)
                                IncompleteFrames.Add(frame);
                            else
                                CompletedFrames.Enqueue(frame);
                            continue;
                        }

                        switch (frame.Type)
                        {
                            // For a complete documentation and rules on fragmentation see http://tools.ietf.org/html/rfc6455#section-5.4
                            // A fragmented Frame's last fragment's opcode is 0 (Continuation) and the FIN bit is set to 1.
                            case WebSocketFrameTypes.Continuation:
                                // Do an assemble pass only if OnFragment is not set. Otherwise put it in the CompletedFrames, we will handle it in the HandleEvent phase.
                                if (OnIncompleteFrame == null)
                                {
                                    frame.Assemble(IncompleteFrames);

                                    // Remove all incomplete frames
                                    IncompleteFrames.Clear();

                                    // Control frames themselves MUST NOT be fragmented. So, its a normal text or binary frame. Go, handle it as usual.
                                    goto case WebSocketFrameTypes.Binary;
                                }
                                else
                                {
                                    CompletedFrames.Enqueue(frame);
                                    ProtocolEventHelper.EnqueueProtocolEvent(new ProtocolEventInfo(this));
                                }
                                break;

                            case WebSocketFrameTypes.Text:
                            case WebSocketFrameTypes.Binary:
                                frame.DecodeWithExtensions(WebSocket);
                                CompletedFrames.Enqueue(frame);
                                ProtocolEventHelper.EnqueueProtocolEvent(new ProtocolEventInfo(this));
                                break;

                            // Upon receipt of a Ping frame, an endpoint MUST send a Pong frame in response, unless it already received a Close frame.
                            case WebSocketFrameTypes.Ping:
                                if (!closeSent && !closed)
                                    Send(new WebSocketFrame(this.WebSocket, WebSocketFrameTypes.Pong, frame.Data));
                                break;

                            case WebSocketFrameTypes.Pong:
                                waitingForPong = false;

                                try
                                {
                                    // Get the ticks from the frame's payload
                                    long ticksSent = BitConverter.ToInt64(frame.Data, 0);

                                    // the difference between the current time and the time when the ping message is sent
                                    TimeSpan diff = TimeSpan.FromTicks(lastMessage.Ticks - ticksSent);

                                    // add it to the buffer
                                    this.rtts.Add((int)diff.TotalMilliseconds);

                                    // and calculate the new latency
                                    this.Latency = CalculateLatency();
                                }
                                catch
                                {
                                    // https://tools.ietf.org/html/rfc6455#section-5.5
                                    // A Pong frame MAY be sent unsolicited.  This serves as a
                                    // unidirectional heartbeat.  A response to an unsolicited Pong frame is
                                    // not expected. 
                                }

                                break;

                            // If an endpoint receives a Close frame and did not previously send a Close frame, the endpoint MUST send a Close frame in response.
                            case WebSocketFrameTypes.ConnectionClose:
                                HTTPManager.Logger.Information("WebSocketResponse", "ConnectionClose packet received!", this.Context);

                                CloseFrame = frame;
                                if (!closeSent)
                                    Send(new WebSocketFrame(this.WebSocket, WebSocketFrameTypes.ConnectionClose, null));
                                closed = true;
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        if (HTTPUpdateDelegator.IsCreated)
                        {
                            this.baseRequest.Exception = e;
                            this.baseRequest.State = HTTPRequestStates.Error;
                        }
                        else
                            this.baseRequest.State = HTTPRequestStates.Aborted;

                        closed = true;
                        newFrameSignal.Set();
                    }
                }

                HTTPManager.Logger.Information("WebSocketResponse", "Ending Read thread! closed: " + closed, this.Context);
            }
            finally
            {
                HTTPManager.Heartbeats.Unsubscribe(this);
                HTTPUpdateDelegator.OnApplicationForegroundStateChanged -= OnApplicationForegroundStateChanged;

                HTTPManager.Logger.Information("WebSocketResponse", "ReceiveThread - Closed!", this.Context);

                TryToCleanup();
            }
        }

        #endregion

        #region Sending Out Events

        /// <summary>
        /// Internal function to send out received messages.
        /// </summary>
        void IProtocol.HandleEvents()
        {
            WebSocketFrameReader frame;
            while (CompletedFrames.TryDequeue(out frame))
            {
                // Bugs in the clients shouldn't interrupt the code, so we need to try-catch and ignore any exception occurring here
                try
                {
                    switch (frame.Type)
                    {
                        case WebSocketFrameTypes.Continuation:
                            HTTPManager.Logger.Verbose("WebSocketResponse", "HandleEvents - OnIncompleteFrame", this.Context);
                            if (OnIncompleteFrame != null)
                                OnIncompleteFrame(this, frame);
                            break;

                        case WebSocketFrameTypes.Text:
                            // Any not Final frame is handled as a fragment
                            if (!frame.IsFinal)
                                goto case WebSocketFrameTypes.Continuation;

                            HTTPManager.Logger.Verbose("WebSocketResponse", "HandleEvents - OnText", this.Context);
                            if (OnText != null)
                                OnText(this, frame.DataAsText);
                            break;

                        case WebSocketFrameTypes.Binary:
                            // Any not Final frame is handled as a fragment
                            if (!frame.IsFinal)
                                goto case WebSocketFrameTypes.Continuation;

                            HTTPManager.Logger.Verbose("WebSocketResponse", "HandleEvents - OnBinary", this.Context);
                            if (OnBinary != null)
                                OnBinary(this, frame.Data);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("WebSocketResponse", "HandleEvents", ex, this.Context);
                }
            }

            // 2015.05.09
            // State checking added because if there is an error the OnClose called first, and then the OnError.
            // Now, when there is an error only the OnError event will be called!
            if (IsClosed && OnClosed != null && baseRequest.State == HTTPRequestStates.Processing)
            {
                HTTPManager.Logger.Verbose("WebSocketResponse", "HandleEvents - Calling OnClosed", this.Context);
                try
                {
                    UInt16 statusCode = 0;
                    string msg = string.Empty;

                    // If we received any data, we will get the status code and the message from it
                    if (/*CloseFrame != null && */CloseFrame.Data != null && CloseFrame.Data.Length >= 2)
                    {
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(CloseFrame.Data, 0, 2);
                        statusCode = BitConverter.ToUInt16(CloseFrame.Data, 0);

                        if (CloseFrame.Data.Length > 2)
                            msg = Encoding.UTF8.GetString(CloseFrame.Data, 2, CloseFrame.Data.Length - 2);

                        CloseFrame.ReleaseData();
                    }

                    OnClosed(this, statusCode, msg);
                    OnClosed = null;
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("WebSocketResponse", "HandleEvents - OnClosed", ex, this.Context);
                }
            }
        }

        #endregion

        #region IHeartbeat Implementation

        void IHeartbeat.OnHeartbeatUpdate(TimeSpan dif)
        {
            DateTime now = DateTime.UtcNow;

            if (!waitingForPong && now - lastMessage >= PingFrequnecy)
                SendPing();

            if (waitingForPong && now - lastPing > this.WebSocket.CloseAfterNoMessage)
            {
                HTTPManager.Logger.Warning("WebSocketResponse", 
                    string.Format("No message received in the given time! Closing WebSocket. LastPing: {0}, PingFrequency: {1}, Close After: {2}, Now: {3}", 
                    this.lastPing, this.PingFrequnecy, this.WebSocket.CloseAfterNoMessage, now), this.Context);

                CloseWithError(HTTPRequestStates.Error, "No message received in the given time!");
            }
        }

        #endregion

        private void OnApplicationForegroundStateChanged(bool isPaused)
        {
            if (!isPaused)
                lastMessage = DateTime.UtcNow;
        }

        private void SendPing()
        {
            HTTPManager.Logger.Information("WebSocketResponse", "Sending Ping frame, waiting for a pong...", this.Context);

            lastPing = DateTime.UtcNow;
            waitingForPong = true;

            try
            {
                long ticks = DateTime.UtcNow.Ticks;
                var ticksBytes = BitConverter.GetBytes(ticks);

                var pingFrame = new WebSocketFrame(this.WebSocket, WebSocketFrameTypes.Ping, ticksBytes);

                Send(pingFrame);
            }
            catch
            {
                HTTPManager.Logger.Information("WebSocketResponse", "Error while sending PING message! Closing WebSocket.", this.Context);
                CloseWithError(HTTPRequestStates.Error, "Error while sending PING message!");
            }
        }

        private void CloseWithError(HTTPRequestStates state, string message)
        {
            if (!string.IsNullOrEmpty(message))
                this.baseRequest.Exception = new Exception(message);
            this.baseRequest.State = state;

            this.closed = true;

            HTTPManager.Heartbeats.Unsubscribe(this);
            HTTPUpdateDelegator.OnApplicationForegroundStateChanged -= OnApplicationForegroundStateChanged;

            CloseStream();
            ProtocolEventHelper.EnqueueProtocolEvent(new ProtocolEventInfo(this));
        }

        private int CalculateLatency()
        {
            if (this.rtts.Count == 0)
                return 0;

            int sumLatency = 0;
            for (int i = 0; i < this.rtts.Count; ++i)
                sumLatency += this.rtts[i];

            return sumLatency / this.rtts.Count;
        }

        void IProtocol.CancellationRequested()
        {
            CloseWithError(HTTPRequestStates.Aborted, null);
        }

        private void TryToCleanup()
        {
            if (Interlocked.Increment(ref this.closedThreads) == 2)
            {
                ProtocolEventHelper.EnqueueProtocolEvent(new ProtocolEventInfo(this));
                (newFrameSignal as IDisposable).Dispose();
                newFrameSignal = null;

                CloseStream();
            }
        }

        public override string ToString()
        {
            return this.ConnectionKey.ToString();
        }
    }
}

#endif
