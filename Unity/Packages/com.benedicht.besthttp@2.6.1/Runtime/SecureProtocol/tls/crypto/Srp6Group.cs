#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto
{
    /// <summary>Carrier class for SRP-6 group parameters.</summary>
    public class Srp6Group
    {
        private readonly BigInteger n, g;

        /// <summary>Base constructor.</summary>
        /// <param name="n">the n value.</param>
        /// <param name="g">the g value.</param>
        public Srp6Group(BigInteger n, BigInteger g)
        {
            this.n = n;
            this.g = g;
        }

        public virtual BigInteger G
        {
            get { return g; }
        }

        public virtual BigInteger N
        {
            get { return n; }
        }
    }
}
#pragma warning restore
#endif
