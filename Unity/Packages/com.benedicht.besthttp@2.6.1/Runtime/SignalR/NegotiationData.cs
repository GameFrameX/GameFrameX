#if !BESTHTTP_DISABLE_SIGNALR

using System;
using System.Collections.Generic;

using BestHTTP.JSON;

namespace BestHTTP.SignalR
{
    public sealed class NegotiationData
    {
        #region Public Negotiate data

        /// <summary>
        /// Path to the SignalR endpoint. Currently not used by the client.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// WebSocket endpoint.
        /// </summary>
        public string WebSocketServerUrl { get; private set; }

        /// <summary>
        /// Connection token assigned by the server. See this article for more details: http://www.asp.net/signalr/overview/security/introduction-to-security#connectiontoken.
        /// This value needs to be sent in each subsequent request as the value of the connectionToken parameter
        /// </summary>
        public string ConnectionToken { get; private set; }

        /// <summary>
        /// The id of the connection.
        /// </summary>
        public string ConnectionId { get; private set; }

        /// <summary>
        /// The amount of time in seconds the client should wait before attempting to reconnect if it has not received a keep alive message.
        /// If the server is configured to not send keep alive messages this value is null.
        /// </summary>
        public TimeSpan? KeepAliveTimeout { get; private set; }

        /// <summary>
        /// The amount of time within which the client should try to reconnect if the connection goes away.
        /// </summary>
        public TimeSpan DisconnectTimeout { get; private set; }

        /// <summary>
        /// Timeout of poll requests.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; private set; }

        /// <summary>
        /// Whether the server supports websockets.
        /// </summary>
        public bool TryWebSockets { get; private set; }

        /// <summary>
        /// The version of the protocol used for communication.
        /// </summary>
        public string ProtocolVersion { get; private set; }

        /// <summary>
        /// The maximum amount of time the client should try to connect to the server using a given transport.
        /// </summary>
        public TimeSpan TransportConnectTimeout { get; private set; }

        /// <summary>
        /// The wait time before restablishing a long poll connection after data is sent from the server. The default value is 0.
        /// </summary>
        public TimeSpan LongPollDelay { get; private set; }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler that called when the negotiation data received and parsed successfully.
        /// </summary>
        public Action<NegotiationData> OnReceived;

        /// <summary>
        /// Event handler that called when an error happens.
        /// </summary>
        public Action<NegotiationData, string> OnError;

        #endregion

        #region Private

        private HTTPRequest NegotiationRequest;
        private IConnection Connection;

        #endregion

        public NegotiationData(Connection connection)
        {
            this.Connection = connection;
        }

        /// <summary>
        /// Start to get the negotiation data.
        /// </summary>
        public void Start()
        {
            NegotiationRequest = new HTTPRequest(Connection.BuildUri(RequestTypes.Negotiate), HTTPMethods.Get, true, true, OnNegotiationRequestFinished);
            Connection.PrepareRequest(NegotiationRequest, RequestTypes.Negotiate);
            NegotiationRequest.Send();

            HTTPManager.Logger.Information("NegotiationData", "Negotiation request sent");
        }

        /// <summary>
        /// Abort the negotiation request.
        /// </summary>
        public void Abort()
        {
            if (NegotiationRequest != null)
            {
                OnReceived = null;
                OnError = null;
                NegotiationRequest.Abort();
            }
        }

        #region Request Event Handler

        private void OnNegotiationRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            NegotiationRequest = null;

            switch (req.State)
            {
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        HTTPManager.Logger.Information("NegotiationData", "Negotiation data arrived: " + resp.DataAsText);

                        int idx = resp.DataAsText.IndexOf("{");
                        if (idx < 0)
                        {
                            RaiseOnError("Invalid negotiation text: " + resp.DataAsText);
                            return;
                        }

                        var Negotiation = Parse(resp.DataAsText.Substring(idx));

                        if (Negotiation == null)
                        {
                            RaiseOnError("Parsing Negotiation data failed: " + resp.DataAsText);
                            return;
                        }

                        if (OnReceived != null)
                        {
                            OnReceived(this);
                            OnReceived = null;
                        }
                    }
                    else
                        RaiseOnError(string.Format("Negotiation request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2} Uri: {3}",
                                                                    resp.StatusCode,
                                                                    resp.Message,
                                                                    resp.DataAsText,
                                                                    req.CurrentUri));
                    break;

                case HTTPRequestStates.Error:
                    RaiseOnError(req.Exception != null ? (req.Exception.Message + " " + req.Exception.StackTrace) : string.Empty);
                    break;

                default:
                    RaiseOnError(req.State.ToString());
                    break;
            }
        }

        #endregion

        #region Helper Methods

        private void RaiseOnError(string err)
        {
            HTTPManager.Logger.Error("NegotiationData", "Negotiation request failed with error: " + err);

            if (OnError != null)
            {
                OnError(this, err);
                OnError = null;
            }
        }

        private NegotiationData Parse(string str)
        {
            bool success = false;
            Dictionary<string, object> dict = Json.Decode(str, ref success) as Dictionary<string, object>;
            if (!success)
                return null;

            try
            {
                this.Url = GetString(dict, "Url");

                if (dict.ContainsKey("webSocketServerUrl"))
                    this.WebSocketServerUrl = GetString(dict, "webSocketServerUrl");

                this.ConnectionToken = Uri.EscapeDataString(GetString(dict, "ConnectionToken"));
                this.ConnectionId = GetString(dict, "ConnectionId");

                if (dict.ContainsKey("KeepAliveTimeout"))
                    this.KeepAliveTimeout =  TimeSpan.FromSeconds(GetDouble(dict, "KeepAliveTimeout"));

                this.DisconnectTimeout = TimeSpan.FromSeconds(GetDouble(dict, "DisconnectTimeout"));

                if (dict.ContainsKey("ConnectionTimeout"))
                    this.ConnectionTimeout = TimeSpan.FromSeconds(GetDouble(dict, "ConnectionTimeout"));
                else
                    this.ConnectionTimeout = TimeSpan.FromSeconds(120);

                this.TryWebSockets = (bool)Get(dict, "TryWebSockets");
                this.ProtocolVersion = GetString(dict, "ProtocolVersion");
                this.TransportConnectTimeout = TimeSpan.FromSeconds(GetDouble(dict, "TransportConnectTimeout"));

                if (dict.ContainsKey("LongPollDelay"))
                    this.LongPollDelay = TimeSpan.FromSeconds(GetDouble(dict, "LongPollDelay"));
            }
            catch(Exception ex)
            {
                HTTPManager.Logger.Exception("NegotiationData", "Parse", ex);
                return null;
            }

            return this;
        }

        private static object Get(Dictionary<string, object> from, string key)
        {
            object value;
            if (!from.TryGetValue(key, out value))
                throw new System.Exception(string.Format("Can't get {0} from Negotiation data!", key));
            return value;
        }

        private static string GetString(Dictionary<string, object> from, string key)
        {
            return Get(from, key) as string;
        }

        private static List<string> GetStringList(Dictionary<string, object> from, string key)
        {
            List<object> value = Get(from, key) as List<object>;

            List<string> result = new List<string>(value.Count);
            for (int i = 0; i < value.Count; ++i)
            {
                string str = value[i] as string;
                if (str != null)
                    result.Add(str);
            }

            return result;
        }

        private static int GetInt(Dictionary<string, object> from, string key)
        {
            return (int)(double)Get(from, key);
        }

        private static double GetDouble(Dictionary<string, object> from, string key)
        {
            return (double)Get(from, key);
        }

        #endregion
    }
}

#endif