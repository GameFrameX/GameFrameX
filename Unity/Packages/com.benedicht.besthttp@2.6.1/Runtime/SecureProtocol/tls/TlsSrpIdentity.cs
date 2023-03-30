#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>Processor interface for an SRP identity.</summary>
    public interface TlsSrpIdentity
    {
        byte[] GetSrpIdentity();

        byte[] GetSrpPassword();
    }
}
#pragma warning restore
#endif
