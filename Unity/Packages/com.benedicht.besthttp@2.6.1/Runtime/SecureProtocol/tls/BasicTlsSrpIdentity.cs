#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>A basic SRP Identity holder.</summary>
    public class BasicTlsSrpIdentity
        : TlsSrpIdentity
    {
        protected readonly byte[] m_identity;
        protected readonly byte[] m_password;

        public BasicTlsSrpIdentity(byte[] identity, byte[] password)
        {
            this.m_identity = Arrays.Clone(identity);
            this.m_password = Arrays.Clone(password);
        }

        public BasicTlsSrpIdentity(string identity, string password)
        {
            this.m_identity = Strings.ToUtf8ByteArray(identity);
            this.m_password = Strings.ToUtf8ByteArray(password);
        }

        public virtual byte[] GetSrpIdentity()
        {
            return m_identity;
        }

        public virtual byte[] GetSrpPassword()
        {
            return m_password;
        }
    }
}
#pragma warning restore
#endif
