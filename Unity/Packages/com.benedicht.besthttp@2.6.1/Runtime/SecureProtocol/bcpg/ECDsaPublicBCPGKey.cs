#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
    /// <remarks>Base class for an ECDSA Public Key.</remarks>
    public class ECDsaPublicBcpgKey
        : ECPublicBcpgKey
    {
        /// <param name="bcpgIn">The stream to read the packet from.</param>
        protected internal ECDsaPublicBcpgKey(
            BcpgInputStream bcpgIn)
            : base(bcpgIn)
        {
        }

        public ECDsaPublicBcpgKey(
            DerObjectIdentifier oid,
            ECPoint point)
            : base(oid, point)
        {
        }

        public ECDsaPublicBcpgKey(
            DerObjectIdentifier oid,
            BigInteger encodedPoint)
            : base(oid, encodedPoint)
        {
        }
    }
}
#pragma warning restore
#endif
