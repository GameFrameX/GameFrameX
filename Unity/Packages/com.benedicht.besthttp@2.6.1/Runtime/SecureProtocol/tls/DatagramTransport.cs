#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>Base interface for an object sending and receiving DTLS data.</summary>
    public interface DatagramTransport
        : DatagramReceiver, DatagramSender, TlsCloseable
    {
    }
}
#pragma warning restore
#endif
