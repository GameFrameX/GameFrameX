using System;
using System.Collections.Generic;

using BestHTTP.Connections;
using BestHTTP.Extensions;
using BestHTTP.Logger;

namespace BestHTTP.Core
{
    public enum HostProtocolSupport : byte
    {
        Unknown = 0x00,
        HTTP1   = 0x01,
        HTTP2   = 0x02
    }

    /// <summary>
    /// A HostConnection object manages the connections to a host and the request queue.
    /// </summary>
    public sealed class HostConnection
    {
        public HostDefinition Host { get; private set; }

        public string VariantId { get; private set; }

        public HostProtocolSupport ProtocolSupport { get; private set; }
        public DateTime LastProtocolSupportUpdate { get; private set; }
        
        public int QueuedRequests { get { return this.Queue.Count; } }

        public LoggingContext Context { get; private set; }

        private List<ConnectionBase> Connections = new List<ConnectionBase>();
        private List<HTTPRequest> Queue = new List<HTTPRequest>();

        public HostConnection(HostDefinition host, string variantId)
        {
            this.Host = host;
            this.VariantId = variantId;

            this.Context = new LoggingContext(this);
            this.Context.Add("Host", this.Host.Host);
            this.Context.Add("VariantId", this.VariantId);
        }

        internal void AddProtocol(HostProtocolSupport protocolSupport)
        {
            this.LastProtocolSupportUpdate = DateTime.UtcNow;

            var oldProtocol = this.ProtocolSupport;

            if (oldProtocol != protocolSupport)
            {
                this.ProtocolSupport = protocolSupport;

                HTTPManager.Logger.Information(typeof(HostConnection).Name, string.Format("AddProtocol({0}) - changing from {1} to {2}", this.VariantId, oldProtocol, protocolSupport), this.Context);

                HostManager.Save();

                TryToSendQueuedRequests();
            }
        }

        internal HostConnection Send(HTTPRequest request)
        {
            var conn = GetNextAvailable(request);

            if (conn != null)
            {
                request.State = HTTPRequestStates.Processing;

                request.Prepare();

                // then start process the request
                conn.Process(request);
            }
            else
            {
                // If no free connection found and creation prohibited, we will put back to the queue
                this.Queue.Add(request);
            }

            return this;
        }

        internal ConnectionBase GetNextAvailable(HTTPRequest request)
        {
            int activeConnections = 0;
            ConnectionBase conn = null;
            // Check the last created connection first. This way, if a higher level protocol is present that can handle more requests (== HTTP/2) that protocol will be chosen
            //  and others will be closed when their inactivity time is reached.
            for (int i = Connections.Count - 1; i >= 0; --i)
            {
                conn = Connections[i];

                if (conn.State == HTTPConnectionStates.Initial || conn.State == HTTPConnectionStates.Free || conn.CanProcessMultiple)
                {
                    if (!conn.TestConnection())
                    {
                        HTTPManager.Logger.Verbose("HostConnection", "GetNextAvailable - TestConnection returned false!", this.Context, request.Context, conn.Context);

                        RemoveConnectionImpl(conn, HTTPConnectionStates.Closed);
                        continue;
                    }

                    HTTPManager.Logger.Verbose("HostConnection", string.Format("GetNextAvailable - returning with connection. state: {0}, CanProcessMultiple: {1}", conn.State, conn.CanProcessMultiple), this.Context, request.Context, conn.Context);
                    return conn;
                }

                activeConnections++;
            }

            if (activeConnections >= HTTPManager.MaxConnectionPerServer)
            {
                HTTPManager.Logger.Verbose("HostConnection", string.Format("GetNextAvailable - activeConnections({0}) >= HTTPManager.MaxConnectionPerServer({1})", activeConnections, HTTPManager.MaxConnectionPerServer), this.Context, request.Context);
                return null;
            }

            string key = HostDefinition.GetKeyForRequest(request);

            conn = null;

#if UNITY_WEBGL && !UNITY_EDITOR
            conn = new WebGLConnection(key);
#else
            if (request.CurrentUri.IsFile)
                conn = new FileConnection(key);
            else
            {
#if !BESTHTTP_DISABLE_ALTERNATE_SSL
                // Hold back the creation of a new connection until we know more about the remote host's features.
                // If we send out multiple requests at once it will execute the first and delay the others. 
                // While it will decrease performance initially, it will prevent the creation of TCP connections
                //  that will be unused after their first request processing if the server supports HTTP/2.
                if (activeConnections >= 1 && (this.ProtocolSupport == HostProtocolSupport.Unknown || this.ProtocolSupport == HostProtocolSupport.HTTP2))
                {
                    HTTPManager.Logger.Verbose("HostConnection", string.Format("GetNextAvailable - waiting for protocol support message. activeConnections: {0}, ProtocolSupport: {1} ", activeConnections, this.ProtocolSupport), this.Context, request.Context);
                    return null;
                }
#endif

                conn = new HTTPConnection(key);
                HTTPManager.Logger.Verbose("HostConnection", string.Format("GetNextAvailable - creating new connection, key: {0} ", key), this.Context, request.Context, conn.Context);
            }
#endif
            Connections.Add(conn);

            return conn;
        }

        internal HostConnection RecycleConnection(ConnectionBase conn)
        {
            conn.State = HTTPConnectionStates.Free;

            BestHTTP.Extensions.Timer.Add(new TimerData(TimeSpan.FromSeconds(1), conn, CloseConnectionAfterInactivity));

            return this;
        }

        private bool RemoveConnectionImpl(ConnectionBase conn, HTTPConnectionStates setState)
        {
            conn.State = setState;
            conn.Dispose();

            bool found = this.Connections.Remove(conn);

            if (!found)
                HTTPManager.Logger.Information(typeof(HostConnection).Name, string.Format("RemoveConnection - Couldn't find connection! key: {0}", conn.ServerAddress), this.Context, conn.Context);

            return found;
        }

        internal HostConnection RemoveConnection(ConnectionBase conn, HTTPConnectionStates setState)
        {
            RemoveConnectionImpl(conn, setState);

            return this;
        }

        internal HostConnection TryToSendQueuedRequests()
        {
            while (this.Queue.Count > 0 && GetNextAvailable(this.Queue[0]) != null)
            {
                Send(this.Queue[0]);
                this.Queue.RemoveAt(0);
            }

            return this;
        }

        public ConnectionBase Find(Predicate<ConnectionBase> match)
        {
            return this.Connections.Find(match);
        }

        private bool CloseConnectionAfterInactivity(DateTime now, object context)
        {
            var conn = context as ConnectionBase;

            bool closeConnection = conn.State == HTTPConnectionStates.Free && now - conn.LastProcessTime >= conn.KeepAliveTime;
            if (closeConnection)
            {
                HTTPManager.Logger.Information(typeof(HostConnection).Name, string.Format("CloseConnectionAfterInactivity - [{0}] Closing! State: {1}, Now: {2}, LastProcessTime: {3}, KeepAliveTime: {4}",
                    conn.ToString(), conn.State, now.ToString(System.Globalization.CultureInfo.InvariantCulture), conn.LastProcessTime.ToString(System.Globalization.CultureInfo.InvariantCulture), conn.KeepAliveTime), this.Context, conn.Context);

                RemoveConnection(conn, HTTPConnectionStates.Closed);
                return false;
            }

            // repeat until the connection's state is free
            return conn.State == HTTPConnectionStates.Free;
        }

        public void RemoveAllIdleConnections()
        {
            for (int i = 0; i < this.Connections.Count; i++)
                if (this.Connections[i].State == HTTPConnectionStates.Free)
                {
                    int countBefore = this.Connections.Count;
                    RemoveConnection(this.Connections[i], HTTPConnectionStates.Closed);

                    if (countBefore != this.Connections.Count)
                        i--;
                }
        }

        internal void Shutdown()
        {
            this.Queue.Clear();

            foreach (var conn in this.Connections)
            {
                // Swallow any exceptions, we are quitting anyway.
                try
                {
                    conn.Shutdown(ShutdownTypes.Immediate);
                }
                catch { }
            }
            //this.Connections.Clear();
        }

        internal void SaveTo(System.IO.BinaryWriter bw)
        {
            bw.Write(this.LastProtocolSupportUpdate.ToBinary());
            bw.Write((byte)this.ProtocolSupport);
        }

        internal void LoadFrom(int version, System.IO.BinaryReader br)
        {
            this.LastProtocolSupportUpdate = DateTime.FromBinary(br.ReadInt64());
            this.ProtocolSupport = (HostProtocolSupport)br.ReadByte();

            if (DateTime.UtcNow - this.LastProtocolSupportUpdate >= TimeSpan.FromDays(1))
            {
                HTTPManager.Logger.Verbose("HostConnection", string.Format("LoadFrom - Too Old! LastProtocolSupportUpdate: {0}, ProtocolSupport: {1}", this.LastProtocolSupportUpdate.ToString(System.Globalization.CultureInfo.InvariantCulture), this.ProtocolSupport), this.Context);
                this.ProtocolSupport = HostProtocolSupport.Unknown;                
            }
            else
                HTTPManager.Logger.Verbose("HostConnection", string.Format("LoadFrom - LastProtocolSupportUpdate: {0}, ProtocolSupport: {1}", this.LastProtocolSupportUpdate.ToString(System.Globalization.CultureInfo.InvariantCulture), this.ProtocolSupport), this.Context);
        }
    }
}
