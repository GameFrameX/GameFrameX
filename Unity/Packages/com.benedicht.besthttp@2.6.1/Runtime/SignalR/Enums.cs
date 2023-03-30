#if !BESTHTTP_DISABLE_SIGNALR

namespace BestHTTP.SignalR
{
    /// <summary>
    /// Possible transport types.
    /// </summary>
    public enum TransportTypes
    {
        /// <summary>
        /// Transport using WebSockets.
        /// </summary>
        WebSocket,

        /// <summary>
        /// Transport using ServerSentEvents protocol.
        /// </summary>
        ServerSentEvents,

        /// <summary>
        /// Transport using long-polling requests.
        /// </summary>
        LongPoll
    }

    /// <summary>
    /// Server sent message types
    /// </summary>
    public enum MessageTypes
    {
        /// <summary>
        /// An empty json object {} sent by the server to check keep alive.
        /// </summary>
        KeepAlive,

        /// <summary>
        /// A no-hub message that contains data.
        /// </summary>
        Data,

        /// <summary>
        /// A message that can hold multiple data message alongside with other information.
        /// </summary>
        Multiple,

        /// <summary>
        /// A method call result.
        /// </summary>
        Result,

        /// <summary>
        /// A message about a failed method call.
        /// </summary>
        Failure,

        /// <summary>
        /// A message with all information to be able to call a method on the client.
        /// </summary>
        MethodCall,

        /// <summary>
        /// A long running server-method's progress.
        /// </summary>
        Progress
    }

    /// <summary>
    /// Possible SignalR Connection states.
    /// </summary>
    public enum ConnectionStates
    {
        /// <summary>
        /// The initial state of the connection.
        /// </summary>
        Initial,

        /// <summary>
        /// The client authenticates itself with the server. This state is skipped if no AuthenticationProvider is present.
        /// </summary>
        Authenticating,

        /// <summary>
        /// The client sent out the negotiation request to the server.
        /// </summary>
        Negotiating,

        /// <summary>
        /// The client received the negotiation data, created the transport and wait's for the transport's connection.
        /// </summary>
        Connecting,

        /// <summary>
        /// The transport connected and started successfully.
        /// </summary>
        Connected,

        /// <summary>
        /// The client started the reconnect process.
        /// </summary>
        Reconnecting,

        /// <summary>
        /// The connection is closed.
        /// </summary>
        Closed
    }

    /// <summary>
    /// Possible types of SignalR requests.
    /// </summary>
    public enum RequestTypes
    {
        /// <summary>
        /// Request to the /negotiate path to negotiate protocol parameters.
        /// </summary>
        Negotiate,

        /// <summary>
        /// Request to the /connect path to connect to the server. With long-polling, it's like a regular poll request.
        /// </summary>
        Connect,

        /// <summary>
        /// Request to the /start path to start the protocol.
        /// </summary>
        Start,

        /// <summary>
        /// Request to the /poll path to get new messages. Not used with the WebSocketTransport.
        /// </summary>
        Poll,

        /// <summary>
        /// Request to the /send path to send a message to the server. Not used with the WebSocketTransport.
        /// </summary>
        Send,

        /// <summary>
        /// Request to the /reconnect path to initiate a reconnection. It's used instead of the Connect type.
        /// </summary>
        Reconnect,

        /// <summary>
        /// Request to the /abort path to close the connection.
        /// </summary>
        Abort,

        /// <summary>
        /// Request to the /ping path to ping the server keeping the asp.net session alive.
        /// </summary>
        Ping
    }

    /// <summary>
    /// Possible states of a transport.
    /// </summary>
    public enum TransportStates
    {
        /// <summary>
        /// Initial state
        /// </summary>
        Initial,

        /// <summary>
        /// Connecting
        /// </summary>
        Connecting,

        /// <summary>
        /// Reconnecting
        /// </summary>
        Reconnecting,

        /// <summary>
        /// Sending Start request
        /// </summary>
        Starting,

        /// <summary>
        /// Start request finished successfully
        /// </summary>
        Started,

        /// <summary>
        /// Sending Abort request
        /// </summary>
        Closing,

        /// <summary>
        /// The transport closed after Abort request sent
        /// </summary>
        Closed
    }
}

#endif