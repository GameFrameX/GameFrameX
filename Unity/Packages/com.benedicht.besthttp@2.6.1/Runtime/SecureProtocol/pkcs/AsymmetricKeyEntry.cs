#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Pkcs
{
    public class AsymmetricKeyEntry
        : Pkcs12Entry
    {
        private readonly AsymmetricKeyParameter key;

		public AsymmetricKeyEntry(
            AsymmetricKeyParameter key)
			: base(BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable())
        {
            this.key = key;
        }

#if !(SILVERLIGHT || PORTABLE || NETFX_CORE)
        [Obsolete]
        public AsymmetricKeyEntry(
            AsymmetricKeyParameter key,
            Hashtable attributes)
			: base(attributes)
        {
            this.key = key;
        }
#endif

        public AsymmetricKeyEntry(
            AsymmetricKeyParameter  key,
            IDictionary             attributes)
			: base(attributes)
        {
            this.key = key;
        }

		public AsymmetricKeyParameter Key
        {
            get { return this.key; }
        }

		public override bool Equals(object obj)
		{
			AsymmetricKeyEntry other = obj as AsymmetricKeyEntry;

			if (other == null)
				return false;

			return key.Equals(other.key);
		}

		public override int GetHashCode()
		{
			return ~key.GetHashCode();
		}
	}
}
#pragma warning restore
#endif
