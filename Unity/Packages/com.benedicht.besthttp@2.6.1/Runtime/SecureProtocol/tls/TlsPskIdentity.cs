#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>Processor interface for a PSK identity.</summary>
    public interface TlsPskIdentity
    {
        void SkipIdentityHint();

        void NotifyIdentityHint(byte[] psk_identity_hint);

        byte[] GetPskIdentity();

        byte[] GetPsk();
    }
}
#pragma warning restore
#endif
