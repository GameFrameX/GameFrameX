#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto
{
    /// <summary>Carrier class for Diffie-Hellman group parameters.</summary>
    public class DHGroup
    {
        private readonly BigInteger g, p, q;
        private readonly int l;

        /// <summary>Base constructor with the prime factor of (p - 1).</summary>
        /// <param name="p">the prime modulus.</param>
        /// <param name="q">specifies the prime factor of (p - 1).</param>
        /// <param name="g">the base generator.</param>
        /// <param name="l"></param>
        public DHGroup(BigInteger p, BigInteger q, BigInteger g, int l)
        {
            this.p = p;
            this.g = g;
            this.q = q;
            this.l = l;
        }

        public virtual BigInteger G
        {
            get { return g; }
        }

        public virtual int L
        {
            get { return l; }
        }

        public virtual BigInteger P
        {
            get { return p; }
        }

        public virtual BigInteger Q
        {
            get { return q; }
        }
    }
}
#pragma warning restore
#endif
