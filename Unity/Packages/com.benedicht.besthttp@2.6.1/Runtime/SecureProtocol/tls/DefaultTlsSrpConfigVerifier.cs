#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    public class DefaultTlsSrpConfigVerifier
        : TlsSrpConfigVerifier
    {
        private static readonly IList DefaultGroups = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateArrayList();

        static DefaultTlsSrpConfigVerifier()
        {
            DefaultGroups.Add(Srp6StandardGroups.rfc5054_1024);
            DefaultGroups.Add(Srp6StandardGroups.rfc5054_1536);
            DefaultGroups.Add(Srp6StandardGroups.rfc5054_2048);
            DefaultGroups.Add(Srp6StandardGroups.rfc5054_3072);
            DefaultGroups.Add(Srp6StandardGroups.rfc5054_4096);
            DefaultGroups.Add(Srp6StandardGroups.rfc5054_6144);
            DefaultGroups.Add(Srp6StandardGroups.rfc5054_8192);
        }

        // IList is (SRP6Group)
        protected readonly IList m_groups;

        /// <summary>Accept only the group parameters specified in RFC 5054 Appendix A.</summary>
        public DefaultTlsSrpConfigVerifier()
            : this(DefaultGroups)
        {
        }

        /// <summary>Specify a custom set of acceptable group parameters.</summary>
        /// <param name="groups">an <see cref="IList"/> of acceptable <see cref="Srp6Group"/>.</param>
        public DefaultTlsSrpConfigVerifier(IList groups)
        {
            this.m_groups = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateArrayList(groups);
        }

        public virtual bool Accept(TlsSrpConfig srpConfig)
        {
            foreach (Srp6Group group in m_groups)
            {
                if (AreGroupsEqual(srpConfig, group))
                    return true;
            }
            return false;
        }

        protected virtual bool AreGroupsEqual(TlsSrpConfig a, Srp6Group b)
        {
            BigInteger[] ng = a.GetExplicitNG();
            return AreParametersEqual(ng[0], b.N) && AreParametersEqual(ng[1], b.G);
        }

        protected virtual bool AreParametersEqual(BigInteger a, BigInteger b)
        {
            return a == b || a.Equals(b);
        }
    }
}
#pragma warning restore
#endif
