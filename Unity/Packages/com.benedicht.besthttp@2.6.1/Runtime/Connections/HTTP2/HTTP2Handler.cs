#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using System;
using System.Collections.Generic;
using System.Threading;

using System.Collections.Concurrent;

using BestHTTP.Extensions;
using BestHTTP.Core;
using BestHTTP.PlatformSupport.Memory;
using BestHTTP.Logger;

namespace BestHTTP.Connections.HTTP2
{
    public sealed class HTTP2Handler : IHTTPRequestHandler
    {
        public bool HasCustomRequestProcessor { get { return true; } }

        public KeepAliveHeader KeepAlive { get { return null; } }

        public bool CanProcessMultiple { get { return this.goAwaySentAt == DateTime.MaxValue && this.isRunning; } }

        // Connection preface starts with the string PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n).
        private static readonly byte[] MAGIC = new byte[24] { 0x50, 0x52, 0x49, 0x20, 0x2a, 0x20, 0x48, 0x54, 0x54, 0x50, 0x2f, 0x32, 0x2e, 0x30, 0x0d, 0x0a, 0x0d, 0x0a, 0x53, 0x4d, 0x0d, 0x0a, 0x0d, 0x0a };
        public const UInt32 MaxValueFor31Bits = 0xFFFFFFFF >> 1;

        public double Latency { get; private set; }

        public HTTP2SettingsManager settings;
        public HPACKEncoder HPACKEncoder;

        public LoggingContext Context { get; private set; }

        private DateTime lastPingSent = DateTime.MinValue;
        private TimeSpan pingFrequency = TimeSpan.MaxValue; // going to be overridden in RunHandler
        private int waitingForPingAck = 0;

        public static int RTTBufferCapacity = 5;
        private CircularBuffer<double> rtts = new CircularBuffer<double>(RTTBufferCapacity);

        private volatile bool isRunning;

        private AutoResetEvent newFrameSignal = new AutoResetEvent(false);

        private ConcurrentQueue<HTTPRequest> requestQueue = new ConcurrentQueue<HTTPRequest>();

        private List<HTTP2Stream> clientInitiatedStreams = new List<HTTP2Stream>();

        private ConcurrentQueue<HTTP2FrameHeaderAndPayload> newFrames = new ConcurrentQueue<HTTP2FrameHeaderAndPayload>();

        private List<HTTP2FrameHeaderAndPayload> outgoingFrames = new List<HTTP2FrameHeaderAndPayload>();

        private UInt32 remoteWindow;
        private DateTime lastInteraction;
        private DateTime goAwaySentAt = DateTime.MaxValue;

        private HTTPConnection conn;
        private int threadExitCount;

        private TimeSpan MaxGoAwayWaitTime { get { return this.goAwaySentAt == DateTime.MaxValue ? TimeSpan.MaxValue : TimeSpan.FromMilliseconds(Math.Max(this.Latency * 2.5, 1500)); } }

        // https://httpwg.org/specs/rfc7540.html#StreamIdentifiers
        // Streams initiated by a client MUST use odd-numbered stream identifiers
        // With an initial value of -1, the first client initiated stream's id going to be 1.
        private long LastStreamId = -1;

        public HTTP2Handler(HTTPConnection conn)
        {
            this.Context = new LoggingContext(this);

            this.conn = conn;
            this.isRunning = true;

            this.settings = new HTTP2SettingsManager(this);

            Process(this.conn.CurrentRequest);
        }

        public void Process(HTTPRequest request)
        {
            HTTPManager.Logger.Information("HTTP2Handler", "Process request called", this.Context, request.Context);

            request.QueuedAt = DateTime.MinValue;
            request.ProcessingStarted = this.lastInteraction = DateTime.UtcNow;

            this.requestQueue.Enqueue(request);

            // Wee might added the request to a dead queue, signaling would be pointless.
            // When the ConnectionEventHelper processes the Close state-change event
            // requests in the queue going to be resent. (We should avoid resending the request just right now,
            // as it might still select this connection/handler resulting in a infinite loop.)
            if (Volatile.Read(ref this.threadExitCount) == 0)
                this.newFrameSignal.Set();
        }

        public void SignalRunnerThread()
        {
            this.newFrameSignal.Set();
        }

        public void RunHandler()
        {
            HTTPManager.Logger.Information("HTTP2Handler", "Processing thread up and running!", this.Context);

            Thread.CurrentThread.Name = "BestHTTP.HTTP2 Process";

            PlatformSupport.Threading.ThreadedRunner.RunLongLiving(ReadThread);

            try
            {
                bool atLeastOneStreamHasAFrameToSend = true;

                this.HPACKEncoder = new HPACKEncoder(this, this.settings);

                // https://httpwg.org/specs/rfc7540.html#InitialWindowSize
                // The connection flow-control window is also 65,535 octets.
                this.remoteWindow = this.settings.RemoteSettings[HTTP2Settings.INITIAL_WINDOW_SIZE];

                // we want to pack as many data as we can in one tcp segment, but setting the buffer's size too high
                //  we might keep data too long and send them in bursts instead of in a steady stream.
                // Keeping it too low might result in a full tcp segment and one with very low payload
                // Is it possible that one full tcp segment sized buffer would be the best, or multiple of it.
                // It would keep the network busy without any fragments. The ethernet layer has a maximum of 1500 bytes,
                // but there's two layers of 20 byte headers each, so as a theoretical maximum it's 1500-20-20 bytes.
                // On the other hand, if the buffer is small (1-2), that means that for larger data, we have to do a lot
                // of system calls, in that case a larger buffer might be better. Still, if we are not cpu bound,
                // a well saturated network might serve us better.
                using (WriteOnlyBufferedStream bufferedStream = new WriteOnlyBufferedStream(this.conn.connector.Stream, 1024 * 1024 /*1500 - 20 - 20*/))
                {
                    // The client connection preface starts with a sequence of 24 octets
                    bufferedStream.Write(MAGIC, 0, MAGIC.Length);

                    // This sequence MUST be followed by a SETTINGS frame (Section 6.5), which MAY be empty.
                    // The client sends the client connection preface immediately upon receipt of a
                    // 101 (Switching Protocols) response (indicating a successful upgrade)
                    // or as the first application data octets of a TLS connection

                    // Set streams' initial window size to its maximum.
                    this.settings.InitiatedMySettings[HTTP2Settings.INITIAL_WINDOW_SIZE] = HTTPManager.HTTP2Settings.InitialStreamWindowSize;
                    this.settings.InitiatedMySettings[HTTP2Settings.MAX_CONCURRENT_STREAMS] = HTTPManager.HTTP2Settings.MaxConcurrentStreams;
                    this.settings.InitiatedMySettings[HTTP2Settings.ENABLE_CONNECT_PROTOCOL] = (uint)(HTTPManager.HTTP2Settings.EnableConnectProtocol ? 1 : 0);
                    this.settings.InitiatedMySettings[HTTP2Settings.ENABLE_PUSH] = 0;
                    this.settings.SendChanges(this.outgoingFrames);
                    this.settings.RemoteSettings.OnSettingChangedEvent += OnRemoteSettingChanged;

                    // The default window size for the whole connection is 65535 bytes,
                    // but we want to set it to the maximum possible value.
                    Int64 initialConnectionWindowSize = HTTPManager.HTTP2Settings.InitialConnectionWindowSize;

                    // yandex.ru returns with an FLOW_CONTROL_ERROR (3) error when the plugin tries to set the connection window to 2^31 - 1
                    // and works only with a maximum value of 2^31 - 10Mib (10 * 1024 * 1024).
                    if (initialConnectionWindowSize == HTTP2Handler.MaxValueFor31Bits)
                        initialConnectionWindowSize -= 10 * 1024 * 1024;

                    Int64 diff = initialConnectionWindowSize - 65535;
                    if (diff > 0)
                        this.outgoingFrames.Add(HTTP2FrameHelper.CreateWindowUpdateFrame(0, (UInt32)diff));

                    this.pingFrequency = HTTPManager.HTTP2Settings.PingFrequency;

                    while (this.isRunning)
                    {
                        DateTime now = DateTime.UtcNow;

                        if (!atLeastOneStreamHasAFrameToSend)
                        {
                            // buffered stream will call flush automatically if its internal buffer is full.
                            // But we have to make it sure that we flush remaining data before we go to sleep.
                            bufferedStream.Flush();

                            // Wait until we have to send the next ping, OR a new frame is received on the read thread.
                            //                lastPingSent             Now           lastPingSent+frequency       lastPingSent+Ping timeout
                            //----|---------------------|---------------|----------------------|----------------------|------------|
                            // lastInteraction                                                                                    lastInteraction + MaxIdleTime

                            var sendPingAt = this.lastPingSent + this.pingFrequency;
                            var timeoutAt = this.lastPingSent + HTTPManager.HTTP2Settings.Timeout;
                            var nextPingInteraction = sendPingAt < timeoutAt ? sendPingAt : timeoutAt;

                            var disconnectByIdleAt = this.lastInteraction + HTTPManager.HTTP2Settings.MaxIdleTime;

                            var nextDueClientInteractionAt = nextPingInteraction < disconnectByIdleAt ? nextPingInteraction : disconnectByIdleAt;
                            int wait = (int)(nextDueClientInteractionAt - now).TotalMilliseconds;

                            wait = (int)Math.Min(wait, this.MaxGoAwayWaitTime.TotalMilliseconds);

                            if (wait >= 1)
                            {
                                if (HTTPManager.Logger.Level <= Logger.Loglevels.All)
                                    HTTPManager.Logger.Information("HTTP2Handler", string.Format("Sleeping for {0:N0}ms", wait), this.Context);
                                this.newFrameSignal.WaitOne(wait);

                                now = DateTime.UtcNow;
                            }
                        }

                        //  Don't send a new ping until a pong isn't received for the last one
                        if (now - this.lastPingSent >= this.pingFrequency && Interlocked.CompareExchange(ref this.waitingForPingAck, 1, 0) == 0)
                        {
                            this.lastPingSent = now;

                            var frame = HTTP2FrameHelper.CreatePingFrame(HTTP2PingFlags.None);
                            BufferHelper.SetLong(frame.Payload, 0, now.Ticks);

                            this.outgoingFrames.Add(frame);
                        }

                        //  If no pong received in a (configurable) reasonable time, treat the connection broken
                        if (this.waitingForPingAck != 0 && now - this.lastPingSent >= HTTPManager.HTTP2Settings.Timeout)
                            throw new TimeoutException("Ping ACK isn't received in time!");

                        // Process received frames
                        HTTP2FrameHeaderAndPayload header;
                        while (this.newFrames.TryDequeue(out header))
                        {
                            if (header.StreamId > 0)
                            {
                                HTTP2Stream http2Stream = FindStreamById(header.StreamId);

                                // Add frame to the stream, so it can process it when its Process function is called
                                if (http2Stream != null)
                                {
                                    http2Stream.AddFrame(header, this.outgoingFrames);
                                }
                                else
                                {
                                    // Error? It's possible that we closed and removed the stream while the server was in the middle of sending frames
                                    if (HTTPManager.Logger.Level == Loglevels.All)
                                        HTTPManager.Logger.Warning("HTTP2Handler", string.Format("No stream found for id: {0}! Can't deliver frame: {1}", header.StreamId, header), this.Context, http2Stream.Context);
                                }
                            }
                            else
                            {
                                switch (header.Type)
                                {
                                    case HTTP2FrameTypes.SETTINGS:
                                        this.settings.Process(header, this.outgoingFrames);

                                        PluginEventHelper.EnqueuePluginEvent(
                                            new PluginEventInfo(PluginEvents.HTTP2ConnectProtocol,
                                                new HTTP2ConnectProtocolInfo(this.conn.LastProcessedUri.Host,
                                                    this.settings.MySettings[HTTP2Settings.ENABLE_CONNECT_PROTOCOL] == 1 && this.settings.RemoteSettings[HTTP2Settings.ENABLE_CONNECT_PROTOCOL] == 1)));
                                        break;

                                    case HTTP2FrameTypes.PING:
                                        var pingFrame = HTTP2FrameHelper.ReadPingFrame(header);

                                        // https://httpwg.org/specs/rfc7540.html#PING
                                        // if it wasn't an ack for our ping, we have to send one
                                        if ((pingFrame.Flags & HTTP2PingFlags.ACK) == 0)
                                        {
                                            var frame = HTTP2FrameHelper.CreatePingFrame(HTTP2PingFlags.ACK);
                                            Array.Copy(pingFrame.OpaqueData, 0, frame.Payload, 0, pingFrame.OpaqueDataLength);

                                            this.outgoingFrames.Add(frame);
                                        }
                                        break;

                                    case HTTP2FrameTypes.WINDOW_UPDATE:
                                        var windowUpdateFrame = HTTP2FrameHelper.ReadWindowUpdateFrame(header);
                                        this.remoteWindow += windowUpdateFrame.WindowSizeIncrement;
                                        break;

                                    case HTTP2FrameTypes.GOAWAY:
                                        // parse the frame, so we can print out detailed information
                                        HTTP2GoAwayFrame goAwayFrame = HTTP2FrameHelper.ReadGoAwayFrame(header);

                                        HTTPManager.Logger.Information("HTTP2Handler", "Received GOAWAY frame: " + goAwayFrame.ToString(), this.Context);

                                        string msg = string.Format("Server closing the connection! Error code: {0} ({1})", goAwayFrame.Error, goAwayFrame.ErrorCode);
                                        for (int i = 0; i < this.clientInitiatedStreams.Count; ++i)
                                            this.clientInitiatedStreams[i].Abort(msg);
                                        this.clientInitiatedStreams.Clear();

                                        // set the running flag to false, so the thread can exit
                                        this.isRunning = false;

                                        this.conn.State = HTTPConnectionStates.Closed;
                                        break;

                                    case HTTP2FrameTypes.ALT_SVC:
                                        //HTTP2AltSVCFrame altSvcFrame = HTTP2FrameHelper.ReadAltSvcFrame(header);

                                        // Implement
                                        //HTTPManager.EnqueuePluginEvent(new PluginEventInfo(PluginEvents.AltSvcHeader, new AltSvcEventInfo(altSvcFrame.Origin, ))
                                        break;
                                }

                                if (header.Payload != null)
                                    BufferPool.Release(header.Payload);
                            }
                        }

                        UInt32 maxConcurrentStreams = Math.Min(HTTPManager.HTTP2Settings.MaxConcurrentStreams, this.settings.RemoteSettings[HTTP2Settings.MAX_CONCURRENT_STREAMS]);

                        // pre-test stream count to lock only when truly needed.
                        if (this.clientInitiatedStreams.Count < maxConcurrentStreams && this.isRunning)
                        {
                            // grab requests from queue
                            HTTPRequest request;
                            while (this.clientInitiatedStreams.Count < maxConcurrentStreams && this.requestQueue.TryDequeue(out request))
                            {
                                // create a new stream
                                var newStream = new HTTP2Stream((UInt32)Interlocked.Add(ref LastStreamId, 2), this, this.settings, this.HPACKEncoder);

                                // process the request
                                newStream.Assign(request);

                                this.clientInitiatedStreams.Add(newStream);
                            }
                        }

                        // send any settings changes
                        this.settings.SendChanges(this.outgoingFrames);

                        atLeastOneStreamHasAFrameToSend = false;

                        // process other streams
                        // Room for improvement Streams should be processed by their priority!
                        for (int i = 0; i < this.clientInitiatedStreams.Count; ++i)
                        {
                            var stream = this.clientInitiatedStreams[i];
                            stream.Process(this.outgoingFrames);

                            // remove closed, empty streams (not enough to check the closed flag, a closed stream still can contain frames to send)
                            if (stream.State == HTTP2StreamStates.Closed && !stream.HasFrameToSend)
                            {
                                this.clientInitiatedStreams.RemoveAt(i--);
                                stream.Removed();
                            }

                            atLeastOneStreamHasAFrameToSend |= stream.HasFrameToSend;

                            this.lastInteraction = DateTime.UtcNow;
                        }

                        // If we encounter a data frame that too large for the current remote window, we have to stop
                        // sending all data frames as we could send smaller data frames before the large ones.
                        // Room for improvement: An improvement would be here to stop data frame sending per-stream.
                        bool haltDataSending = false;

                        if (this.ShutdownType == ShutdownTypes.Running && now - this.lastInteraction >= HTTPManager.HTTP2Settings.MaxIdleTime)
                        {
                            this.lastInteraction = DateTime.UtcNow;
                            HTTPManager.Logger.Information("HTTP2Handler", "Reached idle time, sending GoAway frame!", this.Context);
                            this.outgoingFrames.Add(HTTP2FrameHelper.CreateGoAwayFrame(0, HTTP2ErrorCodes.NO_ERROR));
                            this.goAwaySentAt = DateTime.UtcNow;
                        }
                        
                        // https://httpwg.org/specs/rfc7540.html#GOAWAY
                        // Endpoints SHOULD always send a GOAWAY frame before closing a connection so that the remote peer can know whether a stream has been partially processed or not.
                        if (this.ShutdownType == ShutdownTypes.Gentle)
                        {
                            HTTPManager.Logger.Information("HTTP2Handler", "Connection abort requested, sending GoAway frame!", this.Context);
                        
                            this.outgoingFrames.Clear();
                            this.outgoingFrames.Add(HTTP2FrameHelper.CreateGoAwayFrame(0, HTTP2ErrorCodes.NO_ERROR));
                            this.goAwaySentAt = DateTime.UtcNow;
                        }

                        if (this.isRunning && now - goAwaySentAt >= this.MaxGoAwayWaitTime)
                        {
                            HTTPManager.Logger.Information("HTTP2Handler", "No GoAway frame received back. Really quitting now!", this.Context);
                            this.isRunning = false;
                            conn.State = HTTPConnectionStates.Closed;
                        }

                        uint streamWindowUpdates = 0;

                        // Go through all the collected frames and send them.
                        for (int i = 0; i < this.outgoingFrames.Count; ++i)
                        {
                            var frame = this.outgoingFrames[i];

                            if (HTTPManager.Logger.Level <= Logger.Loglevels.All && frame.Type != HTTP2FrameTypes.DATA /*&& frame.Type != HTTP2FrameTypes.PING*/)
                                HTTPManager.Logger.Information("HTTP2Handler", "Sending frame: " + frame.ToString(), this.Context);

                            // post process frames
                            switch (frame.Type)
                            {
                                case HTTP2FrameTypes.DATA:
                                    if (haltDataSending)
                                        continue;

                                    // if the tracked remoteWindow is smaller than the frame's payload, we stop sending
                                    // data frames until we receive window-update frames
                                    if (frame.PayloadLength > this.remoteWindow)
                                    {
                                        haltDataSending = true;
                                        HTTPManager.Logger.Warning("HTTP2Handler", string.Format("Data sending halted for this round. Remote Window: {0:N0}, frame: {1}", this.remoteWindow, frame.ToString()), this.Context);
                                        continue;
                                    }

                                    break;

                                case HTTP2FrameTypes.WINDOW_UPDATE:
                                    if (frame.StreamId > 0)
                                        streamWindowUpdates += BufferHelper.ReadUInt31(frame.Payload, 0);
                                    break;
                            }

                            this.outgoingFrames.RemoveAt(i--);

                            using (var buffer = HTTP2FrameHelper.HeaderAsBinary(frame))
                                bufferedStream.Write(buffer.Data, 0, buffer.Length);

                            if (frame.PayloadLength > 0)
                            {
                                bufferedStream.Write(frame.Payload, (int)frame.PayloadOffset, (int)frame.PayloadLength);

                                if (!frame.DontUseMemPool)
                                    BufferPool.Release(frame.Payload);
                            }

                            if (frame.Type == HTTP2FrameTypes.DATA)
                                this.remoteWindow -= frame.PayloadLength;
                        }

                        if (streamWindowUpdates > 0)
                        {
                            var frame = HTTP2FrameHelper.CreateWindowUpdateFrame(0, streamWindowUpdates);
                        
                            if (HTTPManager.Logger.Level <= Logger.Loglevels.All)
                                HTTPManager.Logger.Information("HTTP2Handler", "Sending frame: " + frame.ToString(), this.Context);
                        
                            using (var buffer = HTTP2FrameHelper.HeaderAsBinary(frame))
                                bufferedStream.Write(buffer.Data, 0, buffer.Length);
                        
                            bufferedStream.Write(frame.Payload, (int)frame.PayloadOffset, (int)frame.PayloadLength);
                        }

                    } // while (this.isRunning)

                    bufferedStream.Flush();
                }
            }
            catch (Exception ex)
            {
                // Log out the exception if it's a non-expected one.
                if (this.ShutdownType == ShutdownTypes.Running && this.goAwaySentAt == DateTime.MaxValue && HTTPManager.IsQuitting)
                    HTTPManager.Logger.Exception("HTTP2Handler", "Sender thread", ex, this.Context);
            }
            finally
            {
                TryToCleanup();

                HTTPManager.Logger.Information("HTTP2Handler", "Sender thread closing - cleaning up remaining request...", this.Context);

                for (int i = 0; i < this.clientInitiatedStreams.Count; ++i)
                    this.clientInitiatedStreams[i].Abort("Connection closed unexpectedly");
                this.clientInitiatedStreams.Clear();

                HTTPManager.Logger.Information("HTTP2Handler", "Sender thread closing", this.Context);
            }

            try
            {
                if (this.conn != null && this.conn.connector != null)
                {
                    // Works in the new runtime
                    if (this.conn.connector.TopmostStream != null)
                        using (this.conn.connector.TopmostStream) { }

                    // Works in the old runtime
                    if (this.conn.connector.Stream != null)
                        using (this.conn.connector.Stream) { }
                }
            }
            catch
            { }
        }

        private void OnRemoteSettingChanged(HTTP2SettingsRegistry registry, HTTP2Settings setting, uint oldValue, uint newValue)
        {
            switch(setting)
            {
                case HTTP2Settings.INITIAL_WINDOW_SIZE:
                    this.remoteWindow = newValue - (oldValue - this.remoteWindow);
                    break;
            }
        }

        private void ReadThread()
        {
            try
            {
                Thread.CurrentThread.Name = "BestHTTP.HTTP2 Read";
                HTTPManager.Logger.Information("HTTP2Handler", "Reader thread up and running!", this.Context);

                while (this.isRunning)
                {
                    HTTP2FrameHeaderAndPayload header = HTTP2FrameHelper.ReadHeader(this.conn.connector.Stream);

                    if (HTTPManager.Logger.Level <= Logger.Loglevels.Information && header.Type != HTTP2FrameTypes.DATA /*&& header.Type != HTTP2FrameTypes.PING*/)
                        HTTPManager.Logger.Information("HTTP2Handler", "New frame received: " + header.ToString(), this.Context);

                    // Add the new frame to the queue. Processing it on the write thread gives us the advantage that
                    //  we don't have to deal with too much locking.
                    this.newFrames.Enqueue(header);

                    // ping write thread to process the new frame
                    this.newFrameSignal.Set();

                    switch (header.Type)
                    {
                        // Handle pongs on the read thread, so no additional latency is added to the rtt calculation.
                        case HTTP2FrameTypes.PING:
                            var pingFrame = HTTP2FrameHelper.ReadPingFrame(header);

                            if ((pingFrame.Flags & HTTP2PingFlags.ACK) != 0)
                            {
                                if (Interlocked.CompareExchange(ref this.waitingForPingAck, 0, 1) == 0)
                                    break; // waitingForPingAck was 0 == aren't expecting a ping ack!

                                // it was an ack, payload must contain what we sent

                                var ticks = BufferHelper.ReadLong(pingFrame.OpaqueData, 0);

                                // the difference between the current time and the time when the ping message is sent
                                TimeSpan diff = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - ticks);

                                // add it to the buffer
                                this.rtts.Add(diff.TotalMilliseconds);

                                // and calculate the new latency
                                this.Latency = CalculateLatency();

                                HTTPManager.Logger.Verbose("HTTP2Handler", string.Format("Latency: {0:F2}ms, RTT buffer: {1}", this.Latency, this.rtts.ToString()), this.Context);
                            }
                            break;

                        case HTTP2FrameTypes.GOAWAY:
                            // Just exit from this thread. The processing thread will handle the frame too.
                            return;
                    }
                }
            }
            catch //(Exception ex)
            {
                //HTTPManager.Logger.Exception("HTTP2Handler", "", ex, this.Context);

                //this.isRunning = false;
            }
            finally
            {
                TryToCleanup();
                HTTPManager.Logger.Information("HTTP2Handler", "Reader thread closing", this.Context);
            }
        }

        private void TryToCleanup()
        {
            this.isRunning = false;

            // First thread closing notifies the ConnectionEventHelper
            int counter = Interlocked.Increment(ref this.threadExitCount);
            if (counter == 1)
                ConnectionEventHelper.EnqueueConnectionEvent(new ConnectionEventInfo(this.conn, HTTPConnectionStates.Closed));

            // Last thread closes the AutoResetEvent
            if (counter == 2)
            {
                if (this.newFrameSignal != null)
                    this.newFrameSignal.Close();
                this.newFrameSignal = null;
            }
        }

        private double CalculateLatency()
        {
            if (this.rtts.Count == 0)
                return 0;

            double sumLatency = 0;
            for (int i = 0; i < this.rtts.Count; ++i)
                sumLatency += this.rtts[i];

            return sumLatency / this.rtts.Count;
        }

        HTTP2Stream FindStreamById(UInt32 streamId)
        {
            for (int i = 0; i < this.clientInitiatedStreams.Count; ++i)
            {
                var stream = this.clientInitiatedStreams[i];
                if (stream.Id == streamId)
                    return stream;
            }

            return null;
        }

        public ShutdownTypes ShutdownType { get; private set; }

        public void Shutdown(ShutdownTypes type)
        {
            this.ShutdownType = type;

            switch(this.ShutdownType)
            {
                case ShutdownTypes.Gentle:
                    this.newFrameSignal.Set();
                    break;

                case ShutdownTypes.Immediate:
                    this.conn.connector.Stream.Dispose();
                    break;
            }
        }

        public void Dispose()
        {
            HTTPRequest request = null;
            while (this.requestQueue.TryDequeue(out request))
            {
                HTTPManager.Logger.Information("HTTP2Handler", string.Format("Dispose - Request '{0}' IsCancellationRequested: {1}", request.CurrentUri.ToString(), request.IsCancellationRequested.ToString()), this.Context);
                if (request.IsCancellationRequested)
                {
                    request.Response = null;
                    request.State = HTTPRequestStates.Aborted;
                }
                else
                    RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(request, RequestEvents.Resend));
            }
        }
    }
}

#endif
