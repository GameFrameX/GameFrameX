#if !BESTHTTP_DISABLE_SOCKETIO

using System;

namespace BestHTTP.SocketIO3.Events
{
    /// <summary>
    /// Helper class to provide functions to an easy Enum->string conversation of the transport and SocketIO evenet types.
    /// </summary>
    public static class EventNames
    {
        public const string Connect = "connect";
        public const string Disconnect = "disconnect";
        public const string Event = "event";
        public const string Ack = "ack";
        public const string Error = "error";
        public const string BinaryEvent = "binaryevent";
        public const string BinaryAck = "binaryack";

        private static string[] SocketIONames = new string[] { "unknown", "connect", "disconnect", "event", "ack", "error", "binaryevent", "binaryack" };
        private static string[] TransportNames = new string[] { "unknown", "open", "close", "ping", "pong", "message", "upgrade", "noop" };
        private static string[] BlacklistedEvents = new string[] { "connect", "connect_error", "connect_timeout", "disconnect", "error", "reconnect", 
                                                                   "reconnect_attempt", "reconnect_failed", "reconnect_error", "reconnecting" };

        public static string GetNameFor(SocketIOEventTypes type)
        {
            return SocketIONames[(int)type + 1];
        }

        public static string GetNameFor(TransportEventTypes transEvent)
        {
            return TransportNames[(int)transEvent + 1];
        }

        /// <summary>
        /// Checks an event name whether it's blacklisted or not.
        /// </summary>
        public static bool IsBlacklisted(string eventName)
        {
            for (int i = 0; i < BlacklistedEvents.Length; ++i)
                if (string.Compare(BlacklistedEvents[i], eventName, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            return false;
        }
    }
}

#endif