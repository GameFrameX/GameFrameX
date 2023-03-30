#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.Collections.Generic;

using BestHTTP.Connections.TLS.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls;

namespace BestHTTP.Connections.TLS
{
    public class DefaultTls13Client : AbstractTls13Client
    {
        public DefaultTls13Client(HTTPRequest request, List<ServerName> sniServerNames, List<ProtocolName> protocols)
            : base(request, sniServerNames, protocols, new FastTlsCrypto(new SecureRandom()))
        {
        }
    }
}
#endif
