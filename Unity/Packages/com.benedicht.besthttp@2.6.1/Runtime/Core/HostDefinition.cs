using System;
using System.Collections.Generic;

namespace BestHTTP.Core
{
    public sealed class HostDefinition
    {
        public string Host { get; private set; }
        
        // alt-svc support:
        //  1. When a request receives an alt-svc header send a plugin msg to the manager with all the details to route to the proper hostDefinition.
        //  2. HostDefinition parses the header value
        //  3. If there's at least one supported protocol found, start open a connection to that said alternate
        //  4. If the new connection is open, route new requests to that connection
        public List<HostConnection> Alternates;

        /// <summary>
        /// Requests to the same host can require different connections: http, https, http + proxy, https + proxy, http2, http2 + proxy
        /// </summary>
        public Dictionary<string, HostConnection> hostConnectionVariant = new Dictionary<string, HostConnection>();

        public HostDefinition(string host)
        {
            this.Host = host;
        }

        public HostConnection HasBetterAlternate(HTTPRequest request)
        {
            return null;
        }

        public HostConnection GetHostDefinition(HTTPRequest request)
        {
            string key = GetKeyForRequest(request);

            return GetHostDefinition(key);
        }

        public HostConnection GetHostDefinition(string key)
        {
            HostConnection host = null;

            if (!this.hostConnectionVariant.TryGetValue(key, out host))
                this.hostConnectionVariant.Add(key, host = new HostConnection(this, key));

            return host;
        }

        public void Send(HTTPRequest request)
        {
            GetHostDefinition(request)
                .Send(request);
        }

        public void TryToSendQueuedRequests()
        {
            foreach (var kvp in hostConnectionVariant)
                kvp.Value.TryToSendQueuedRequests();
        }

        public void HandleAltSvcHeader(HTTPResponse response)
        {
            var headerValues = response.GetHeaderValues("alt-svc");
            if (headerValues == null)
                HTTPManager.Logger.Warning(typeof(HostDefinition).Name, "Received HandleAltSvcHeader message, but no Alt-Svc header found!", response.Context);
        }

        public void HandleConnectProtocol(HTTP2ConnectProtocolInfo info)
        {
            HTTPManager.Logger.Information(typeof(HostDefinition).Name, string.Format("Received HandleConnectProtocol message. Connect protocol for host {0}. Enabled: {1}", info.Host, info.Enabled));
        }

        internal void Shutdown()
        {
            foreach (var kvp in this.hostConnectionVariant)
            {
                kvp.Value.Shutdown();
            }
        }

        internal void SaveTo(System.IO.BinaryWriter bw)
        {
            bw.Write(this.hostConnectionVariant.Count);

            foreach (var kvp in this.hostConnectionVariant)
            {
                bw.Write(kvp.Key.ToString());

                kvp.Value.SaveTo(bw);
            }
        }

        internal void LoadFrom(int version, System.IO.BinaryReader br)
        {
            int count = br.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                GetHostDefinition(br.ReadString())
                    .LoadFrom(version, br);
            }
        }

        private static System.Text.StringBuilder keyBuilder = new System.Text.StringBuilder(11);

        // While a ReaderWriterLockSlim would be best with read and write locking and we use only WriteLock, it's still a lightweight locking mechanism instead of the lock statement.
        private static System.Threading.ReaderWriterLockSlim keyBuilderLock = new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.NoRecursion);

        public static string GetKeyForRequest(HTTPRequest request)
        {
            return GetKeyFor(request.CurrentUri
#if !BESTHTTP_DISABLE_PROXY
                , request.Proxy
#endif
                );
        }

        public static string GetKeyFor(Uri uri
#if !BESTHTTP_DISABLE_PROXY
            , Proxy proxy
#endif
            )
        {
            if (uri.IsFile)
                return uri.ToString();

            keyBuilderLock.EnterWriteLock();

            try
            {
                keyBuilder.Length = 0;

#if !BESTHTTP_DISABLE_PROXY
                if (proxy != null && proxy.UseProxyForAddress(uri))
                {
                    keyBuilder.Append(proxy.Address.Scheme);
                    keyBuilder.Append("://");
                    keyBuilder.Append(proxy.Address.Host);
                    keyBuilder.Append(":");
                    keyBuilder.Append(proxy.Address.Port);
                    keyBuilder.Append(" @ ");
                }
#endif

                keyBuilder.Append(uri.Scheme);
                keyBuilder.Append("://");
                keyBuilder.Append(uri.Host);
                keyBuilder.Append(":");
                keyBuilder.Append(uri.Port);

                return keyBuilder.ToString();
            }
            finally
            {
                keyBuilderLock.ExitWriteLock();
            }
        }
    }
}
