#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crmf
{
    /// <summary>
    /// Generic interface for a CertificateRequestMessage control value.
    /// </summary>
    public interface IControl
    {
        /// <summary>
        /// Return the type of this control.
        /// </summary>
        DerObjectIdentifier Type { get; }

        /// <summary>
        /// Return the value contained in this control object.
        /// </summary>
        Asn1Encodable Value { get; }
    }
}
#pragma warning restore
#endif
