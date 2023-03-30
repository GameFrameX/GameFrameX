using System;
using System.Collections.Concurrent;

using BestHTTP.Connections;
using BestHTTP.Logger;

// Required for ConcurrentQueue.Clear extension.
using BestHTTP.Extensions;

namespace BestHTTP.Core
{
    public enum ConnectionEvents
    {
        StateChange,
        ProtocolSupport
    }

    public
#if CSHARP_7_OR_LATER
        readonly
#endif
        struct ConnectionEventInfo
    {
        public readonly ConnectionBase Source;

        public readonly ConnectionEvents Event;

        public readonly HTTPConnectionStates State;

        public readonly HostProtocolSupport ProtocolSupport;

        public readonly HTTPRequest Request;

        public ConnectionEventInfo(ConnectionBase sourceConn, ConnectionEvents @event)
        {
            this.Source = sourceConn;
            this.Event = @event;

            this.State = HTTPConnectionStates.Initial;

            this.ProtocolSupport = HostProtocolSupport.Unknown;

            this.Request = null;
        }

        public ConnectionEventInfo(ConnectionBase sourceConn, HTTPConnectionStates newState)
        {
            this.Source = sourceConn;

            this.Event = ConnectionEvents.StateChange;

            this.State = newState;

            this.ProtocolSupport = HostProtocolSupport.Unknown;

            this.Request = null;
        }

        public ConnectionEventInfo(ConnectionBase sourceConn, HostProtocolSupport protocolSupport)
        {
            this.Source = sourceConn;
            this.Event = ConnectionEvents.ProtocolSupport;

            this.State = HTTPConnectionStates.Initial;

            this.ProtocolSupport = protocolSupport;

            this.Request = null;
        }

        public ConnectionEventInfo(ConnectionBase sourceConn, HTTPRequest request)
        {
            this.Source = sourceConn;

            this.Event = ConnectionEvents.StateChange;

            this.State = HTTPConnectionStates.ClosedResendRequest;

            this.ProtocolSupport = HostProtocolSupport.Unknown;

            this.Request = request;
        }

        public override string ToString()
        {
            return string.Format("[ConnectionEventInfo SourceConnection: {0}, Event: {1}, State: {2}, ProtocolSupport: {3}]",
                this.Source.ToString(), this.Event, this.State, this.ProtocolSupport);
        }
    }

    public static class ConnectionEventHelper
    {
        private static ConcurrentQueue<ConnectionEventInfo> connectionEventQueue = new ConcurrentQueue<ConnectionEventInfo>();

#pragma warning disable 0649
        public static Action<ConnectionEventInfo> OnEvent;
#pragma warning restore

        public static void EnqueueConnectionEvent(ConnectionEventInfo @event)
        {
            if (HTTPManager.Logger.Level == Loglevels.All)
                HTTPManager.Logger.Information("ConnectionEventHelper", "Enqueue connection event: " + @event.ToString(), @event.Source.Context);

            connectionEventQueue.Enqueue(@event);
        }

        internal static void Clear()
        {
            connectionEventQueue.Clear();
        }

        internal static void ProcessQueue()
        {
            ConnectionEventInfo connectionEvent;
            while (connectionEventQueue.TryDequeue(out connectionEvent))
            {
                if (HTTPManager.Logger.Level == Loglevels.All)
                    HTTPManager.Logger.Information("ConnectionEventHelper", "Processing connection event: " + connectionEvent.ToString(), connectionEvent.Source.Context);

                if (OnEvent != null)
                {
                    try
                    {
                        OnEvent(connectionEvent);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("ConnectionEventHelper", "ProcessQueue", ex, connectionEvent.Source.Context);
                    }
                }

                switch (connectionEvent.Event)
                {
                    case ConnectionEvents.StateChange:
                        HandleConnectionStateChange(connectionEvent);
                        break;

                    case ConnectionEvents.ProtocolSupport:
                        HostManager.GetHost(connectionEvent.Source.LastProcessedUri.Host)
                            .GetHostDefinition(connectionEvent.Source.ServerAddress)
                            .AddProtocol(connectionEvent.ProtocolSupport);
                        break;
                }
            }
        }

        private static void HandleConnectionStateChange(ConnectionEventInfo @event)
        {
            var connection = @event.Source;

            switch (@event.State)
            {
                case HTTPConnectionStates.Recycle:
                    HostManager.GetHost(connection.LastProcessedUri.Host)
                        .GetHostDefinition(connection.ServerAddress)
                        .RecycleConnection(connection)
                        .TryToSendQueuedRequests();

                    break;

                case HTTPConnectionStates.WaitForProtocolShutdown:
                    HostManager.GetHost(connection.LastProcessedUri.Host)
                        .GetHostDefinition(connection.ServerAddress)
                        .RemoveConnection(connection, @event.State);
                    break;

                case HTTPConnectionStates.Closed:
                case HTTPConnectionStates.ClosedResendRequest:
                    // in case of ClosedResendRequest
                    if (@event.Request != null)
                        RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(@event.Request, RequestEvents.Resend));

                    HostManager.GetHost(connection.LastProcessedUri.Host)
                        .GetHostDefinition(connection.ServerAddress)
                        .RemoveConnection(connection, @event.State)
                        .TryToSendQueuedRequests();
                    break;
            }
        }
    }
}
