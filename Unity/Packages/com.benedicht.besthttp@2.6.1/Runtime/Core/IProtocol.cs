using System;

using BestHTTP.Logger;

namespace BestHTTP.Core
{
    public struct HostConnectionKey
    {
        public readonly string Host;
        public readonly string Connection;

        public HostConnectionKey(string host, string connection)
        {
            this.Host = host;
            this.Connection = connection;
        }

        public override string ToString()
        {
            return string.Format("[HostConnectionKey Host: '{0}', Connection: '{1}']", this.Host, this.Connection);
        }
    }

    public interface IProtocol : IDisposable
    {
        HostConnectionKey ConnectionKey { get; }

        bool IsClosed { get; }
        LoggingContext LoggingContext { get; }

        void HandleEvents();

        void CancellationRequested();
    }
}
