#if !BESTHTTP_DISABLE_SOCKETIO

using System;
using System.Text;

using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.SocketIO3.Transports
{
    public sealed class PollingTransport : ITransport
    {
        #region Public (ITransport) Properties

        public TransportTypes Type { get { return TransportTypes.Polling; } }
        public TransportStates State { get; private set; }
        public SocketManager Manager { get; private set; }
        public bool IsRequestInProgress { get { return LastRequest != null; } }
        public bool IsPollingInProgress { get { return PollRequest != null; } }

        #endregion

        #region Private Fields

        /// <summary>
        /// The last POST request we sent to the server.
        /// </summary>
        private HTTPRequest LastRequest;

        /// <summary>
        /// Last GET request we sent to the server.
        /// </summary>
        private HTTPRequest PollRequest;

        #endregion

        public PollingTransport(SocketManager manager)
        {
            Manager = manager;
        }

        public void Open()
        {
            string format = "{0}?EIO={1}&transport=polling&t={2}-{3}{5}";
            if (Manager.Handshake != null)
                format += "&sid={4}";

            bool sendAdditionalQueryParams = !Manager.Options.QueryParamsOnlyForHandshake || (Manager.Options.QueryParamsOnlyForHandshake && Manager.Handshake == null);

            HTTPRequest request = new HTTPRequest(new Uri(string.Format(format,
                                                                        Manager.Uri.ToString(),
                                                                        Manager.ProtocolVersion,
                                                                        Manager.Timestamp.ToString(),
                                                                        Manager.RequestCounter++.ToString(),
                                                                        Manager.Handshake != null ? Manager.Handshake.Sid : string.Empty,
                                                                        sendAdditionalQueryParams ? Manager.Options.BuildQueryParams() : string.Empty)),
                                                OnRequestFinished);

#if !BESTHTTP_DISABLE_CACHING
            // Don't even try to cache it
            request.DisableCache = true;
#endif

            request.MaxRetries = 0;

            if (this.Manager.Options.HTTPRequestCustomizationCallback != null)
                this.Manager.Options.HTTPRequestCustomizationCallback(this.Manager, request);

            request.Send();

            State = TransportStates.Opening;
        }

        /// <summary>
        /// Closes the transport and cleans up resources.
        /// </summary>
        public void Close()
        {
            if (State == TransportStates.Closed)
                return;

            State = TransportStates.Closed;
        }

        #region Packet Sending Implementation

        private System.Collections.Generic.List<OutgoingPacket> lonelyPacketList = new System.Collections.Generic.List<OutgoingPacket>(1);
        public void Send(OutgoingPacket packet)
        {
            try
            {
                lonelyPacketList.Add(packet);
                Send(lonelyPacketList);
            }
            finally
            {
                lonelyPacketList.Clear();
            }
        }

        public void Send(System.Collections.Generic.List<OutgoingPacket> packets)
        {
            if (State != TransportStates.Opening && State != TransportStates.Open)
                return;

            if (IsRequestInProgress)
                throw new Exception("Sending packets are still in progress!");

            

            LastRequest = new HTTPRequest(new Uri(string.Format("{0}?EIO={1}&transport=polling&t={2}-{3}&sid={4}{5}",
                                                                 Manager.Uri.ToString(),
                                                                 Manager.ProtocolVersion,
                                                                 Manager.Timestamp.ToString(),
                                                                 Manager.RequestCounter++.ToString(),
                                                                 Manager.Handshake.Sid,
                                                                 !Manager.Options.QueryParamsOnlyForHandshake ? Manager.Options.BuildQueryParams() : string.Empty)),
                                          HTTPMethods.Post,
                                          OnRequestFinished);


#if !BESTHTTP_DISABLE_CACHING
            // Don't even try to cache it
            LastRequest.DisableCache = true;
#endif
            EncodePackets(packets, LastRequest);

            if (this.Manager.Options.HTTPRequestCustomizationCallback != null)
                this.Manager.Options.HTTPRequestCustomizationCallback(this.Manager, LastRequest);

            LastRequest.Send();
        }

        StringBuilder sendBuilder = new StringBuilder();
        private void EncodePackets(System.Collections.Generic.List<OutgoingPacket> packets, HTTPRequest request)
        {
            sendBuilder.Length = 0;

            for (int i = 0; i < packets.Count; ++i)
            {
                var packet = packets[i];

                if (packet.IsBinary)
                {
                    sendBuilder.Append('b');
                    sendBuilder.Append(Convert.ToBase64String(packet.PayloadData.Data, packet.PayloadData.Offset, packet.PayloadData.Count));
                }
                else
                {
                    sendBuilder.Append(packet.Payload);
                }

                if (packet.Attachements != null)
                {
                    for (int cv = 0; cv < packet.Attachements.Count; ++cv)
                    {
                        sendBuilder.Append((char)0x1E);
                        sendBuilder.Append('b');

                        sendBuilder.Append(Convert.ToBase64String(packet.Attachements[cv]));
                    }
                }

                if (i < packets.Count - 1)
                    sendBuilder.Append((char)0x1E);
            }

            string result = sendBuilder.ToString();
            var length = System.Text.Encoding.UTF8.GetByteCount(result);
            var buffer = BufferPool.Get(length, true);

            System.Text.Encoding.UTF8.GetBytes(result, 0, result.Length, buffer, 0);

            var stream = new BufferSegmentStream();

            stream.Write(new BufferSegment(buffer, 0, length));

            request.UploadStream = stream;
            request.SetHeader("Content-Type", "text/plain; charset=UTF-8");
        }

        private void OnRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            // Clear out the LastRequest variable, so we can start sending out new packets
            LastRequest = null;

            if (State == TransportStates.Closed)
                return;

            string errorString = null;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (HTTPManager.Logger.Level <= BestHTTP.Logger.Loglevels.All)
                        HTTPManager.Logger.Verbose("PollingTransport", "OnRequestFinished: " + resp.DataAsText, this.Manager.Context);

                    if (resp.IsSuccess)
                    {
                        // When we are sending data, the response is an 'ok' string
                        if (req.MethodType != HTTPMethods.Post)
                            ParseResponse(resp);
                    }
                    else
                        errorString = string.Format("Polling - Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}",
                                                        resp.StatusCode,
                                                        resp.Message,
                                                        resp.DataAsText,
                                                        req.CurrentUri);
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    errorString = (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    errorString = string.Format("Polling - Request({0}) Aborted!", req.CurrentUri);
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    errorString = string.Format("Polling - Connection Timed Out! Uri: {0}", req.CurrentUri);
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    errorString = string.Format("Polling - Processing the request({0}) Timed Out!", req.CurrentUri);
                    break;
            }

            if (!string.IsNullOrEmpty(errorString))
                (Manager as IManager).OnTransportError(this, errorString);
        }

        #endregion

        #region Polling Implementation

        public void Poll()
        {
            if (PollRequest != null || State == TransportStates.Paused)
                return;

            PollRequest = new HTTPRequest(new Uri(string.Format("{0}?EIO={1}&transport=polling&t={2}-{3}&sid={4}{5}",
                                                                Manager.Uri.ToString(),
                                                                Manager.ProtocolVersion,
                                                                Manager.Timestamp.ToString(),
                                                                Manager.RequestCounter++.ToString(),
                                                                Manager.Handshake.Sid,
                                                                !Manager.Options.QueryParamsOnlyForHandshake ? Manager.Options.BuildQueryParams() : string.Empty)),
                                        HTTPMethods.Get,
                                        OnPollRequestFinished);

#if !BESTHTTP_DISABLE_CACHING
            // Don't even try to cache it
            PollRequest.DisableCache = true;
#endif

            PollRequest.MaxRetries = 0;

            if (this.Manager.Options.HTTPRequestCustomizationCallback != null)
                this.Manager.Options.HTTPRequestCustomizationCallback(this.Manager, PollRequest);

            PollRequest.Send();
        }

        private void OnPollRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            // Clear the PollRequest variable, so we can start a new poll.
            PollRequest = null;

            if (State == TransportStates.Closed)
                return;

            string errorString = null;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:

                    if (HTTPManager.Logger.Level <= BestHTTP.Logger.Loglevels.All)
                        HTTPManager.Logger.Verbose("PollingTransport", "OnPollRequestFinished: " + resp.DataAsText, this.Manager.Context);

                    if (resp.IsSuccess)
                        ParseResponse(resp);
                    else
                        errorString = string.Format("Polling - Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}",
                                                            resp.StatusCode,
                                                            resp.Message,
                                                            resp.DataAsText,
                                                            req.CurrentUri);
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    errorString = req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception";
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    errorString = string.Format("Polling - Request({0}) Aborted!", req.CurrentUri);
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    errorString = string.Format("Polling - Connection Timed Out! Uri: {0}", req.CurrentUri);
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    errorString = string.Format("Polling - Processing the request({0}) Timed Out!", req.CurrentUri);
                    break;
            }

            if (!string.IsNullOrEmpty(errorString))
                (Manager as IManager).OnTransportError(this, errorString);
        }

        #endregion

        #region Packet Parsing and Handling

        /// <summary>
        /// Preprocessing and sending out packets to the manager.
        /// </summary>
        private void OnPacket(IncomingPacket packet)
        {
            switch (packet.TransportEvent)
            {
                case TransportEventTypes.Open:
                    if (this.State != TransportStates.Opening)
                        HTTPManager.Logger.Warning("PollingTransport", "Received 'Open' packet while state is '" + State.ToString() + "'", this.Manager.Context);
                    else
                        State = TransportStates.Open;
                    goto default;

                case TransportEventTypes.Message:
                    if (packet.SocketIOEvent == SocketIOEventTypes.Connect) //2:40
                        this.State = TransportStates.Open;
                    goto default;

                default:
                    (Manager as IManager).OnPacket(packet);
                    break;
            }
        }

        private void ParseResponse(HTTPResponse resp)
        {
            try
            {
                if (resp == null || resp.Data == null || resp.Data.Length < 1)
                    return;
                
                int idx = 0;
                while (idx < resp.Data.Length)
                {
                    int endIdx = FindNextRecordSeparator(resp.Data, idx);
                    int length = endIdx - idx;

                    if (length <= 0)
                        break;

                    IncomingPacket packet = IncomingPacket.Empty;

                    if (resp.Data[idx] == 'b')
                    {
                        // First byte is the binary indicator('b'). We must skip it, so we advance our idx and also have to decrease length
                        idx++;
                        length--;
                        var base64Encoded = System.Text.Encoding.UTF8.GetString(resp.Data, idx, length);
                        var byteData = Convert.FromBase64String(base64Encoded);
                        packet = this.Manager.Parser.Parse(this.Manager, new BufferSegment(byteData, 0, byteData.Length));
                    }
                    else
                    {
                        // It's the handshake data?
                        if (this.State == TransportStates.Opening)
                        {
                            TransportEventTypes transportEvent = (TransportEventTypes)(resp.Data[idx] - '0');
                            if (transportEvent == TransportEventTypes.Open)
                            {
                                var handshake = BestHTTP.JSON.LitJson.JsonMapper.ToObject<HandshakeData>(Encoding.UTF8.GetString(resp.Data, idx + 1, length - 1));
                                packet = new IncomingPacket(TransportEventTypes.Open, SocketIOEventTypes.Unknown, "/", -1);
                                packet.DecodedArg = handshake;
                            }
                            else
                            {
                                // TODO: error?
                            }
                        }
                        else
                        {
                            packet = this.Manager.Parser.Parse(this.Manager, System.Text.Encoding.UTF8.GetString(resp.Data, idx, length));
                        }
                    }

                    if (!packet.Equals(IncomingPacket.Empty))
                    {
                        try
                        {
                            OnPacket(packet);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("PollingTransport", "ParseResponse - OnPacket", ex, this.Manager.Context);
                            (Manager as IManager).EmitError(ex.Message + " " + ex.StackTrace);
                        }
                    }

                    idx = endIdx + 1;
                }
            }
            catch (Exception ex)
            {
                (Manager as IManager).EmitError(ex.Message + " " + ex.StackTrace);

                HTTPManager.Logger.Exception("PollingTransport", "ParseResponse", ex, this.Manager.Context);
            }
        }

        private int FindNextRecordSeparator(byte[] data, int startIdx)
        {
            for (int i = startIdx; i < data.Length; ++i)
            {
                if (data[i] == 0x1E)
                    return i;
            }
            return data.Length;
        }

        #endregion
    }
}

#endif
