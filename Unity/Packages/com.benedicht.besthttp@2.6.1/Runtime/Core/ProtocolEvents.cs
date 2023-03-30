using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using BestHTTP.Logger;

// Required for ConcurrentQueue.Clear extension.
using BestHTTP.Extensions;

namespace BestHTTP.Core
{
    public
#if CSHARP_7_OR_LATER
        readonly
#endif
        struct ProtocolEventInfo
    {
        public readonly IProtocol Source;

        public ProtocolEventInfo(IProtocol source)
        {
            this.Source = source;
        }

        public override string ToString()
        {
            return string.Format("[ProtocolEventInfo Source: {0}]", Source);
        }
    }

    public static class ProtocolEventHelper
    {
        private static ConcurrentQueue<ProtocolEventInfo> protocolEvents = new ConcurrentQueue<ProtocolEventInfo>();
        private static List<IProtocol> ActiveProtocols = new List<IProtocol>(2);

#pragma warning disable 0649
        public static Action<ProtocolEventInfo> OnEvent;
#pragma warning restore

        public static void EnqueueProtocolEvent(ProtocolEventInfo @event)
        {
            if (HTTPManager.Logger.Level == Loglevels.All)
                HTTPManager.Logger.Information("ProtocolEventHelper", "Enqueue protocol event: " + @event.ToString(), @event.Source.LoggingContext);

            protocolEvents.Enqueue(@event);
        }

        internal static void Clear()
        {
            protocolEvents.Clear();
        }

        internal static void ProcessQueue()
        {
            ProtocolEventInfo protocolEvent;
            while (protocolEvents.TryDequeue(out protocolEvent))
            {
                if (HTTPManager.Logger.Level == Loglevels.All)
                    HTTPManager.Logger.Information("ProtocolEventHelper", "Processing protocol event: " + protocolEvent.ToString(), protocolEvent.Source.LoggingContext);

                if (OnEvent != null)
                {
                    try
                    {
                        OnEvent(protocolEvent);
                    }
                    catch (Exception ex)
{
                        HTTPManager.Logger.Exception("ProtocolEventHelper", "ProcessQueue", ex, protocolEvent.Source.LoggingContext);
                    }
                }

                IProtocol protocol = protocolEvent.Source;

                protocol.HandleEvents();

                if (protocol.IsClosed)
                {
                    ActiveProtocols.Remove(protocol);

                    HostManager.GetHost(protocol.ConnectionKey.Host)
                        .GetHostDefinition(protocol.ConnectionKey.Connection)
                        .TryToSendQueuedRequests();

                    protocol.Dispose();
                }
            }
        }

        internal static void AddProtocol(IProtocol protocol)
        {
            ActiveProtocols.Add(protocol);
        }

        internal static void CancelActiveProtocols()
        {
            for (int i = 0; i < ActiveProtocols.Count; ++i)
            {
                var protocol = ActiveProtocols[i];

                protocol.CancellationRequested();
            }
        }
    }
}
