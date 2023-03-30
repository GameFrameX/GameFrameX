#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto
{
    /// <summary>Carrier class for Elliptic Curve parameter configuration.</summary>
    public class TlsECConfig
    {
        protected readonly int m_namedGroup;

        public TlsECConfig(int namedGroup)
        {
            this.m_namedGroup = namedGroup;
        }

        /// <summary>Return the group used.</summary>
        /// <returns>the <see cref="NamedGroup">named group</see> used.</returns>
        public virtual int NamedGroup
        {
            get { return m_namedGroup; }
        }
    }
}
#pragma warning restore
#endif
