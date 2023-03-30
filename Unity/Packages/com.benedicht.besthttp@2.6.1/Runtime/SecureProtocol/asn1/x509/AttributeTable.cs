#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509
{
    public class AttributeTable
    {
        private readonly IDictionary attributes;

        public AttributeTable(
            IDictionary attrs)
        {
            this.attributes = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable(attrs);
        }

#if !(SILVERLIGHT || PORTABLE || NETFX_CORE)
        [Obsolete]
        public AttributeTable(
            Hashtable attrs)
        {
            this.attributes = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable(attrs);
        }
#endif

		public AttributeTable(
            Asn1EncodableVector v)
        {
            this.attributes = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable(v.Count);

			for (int i = 0; i != v.Count; i++)
            {
                AttributeX509 a = AttributeX509.GetInstance(v[i]);

				attributes.Add(a.AttrType, a);
            }
        }

		public AttributeTable(
            Asn1Set s)
        {
            this.attributes = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable(s.Count);

			for (int i = 0; i != s.Count; i++)
            {
                AttributeX509 a = AttributeX509.GetInstance(s[i]);

				attributes.Add(a.AttrType, a);
            }
        }

		public AttributeX509 Get(
            DerObjectIdentifier oid)
        {
            return (AttributeX509) attributes[oid];
        }

#if !(SILVERLIGHT || PORTABLE || NETFX_CORE)

		public Hashtable ToHashtable()
        {
            return new Hashtable(attributes);
        }
#endif

        public IDictionary ToDictionary()
        {
            return BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable(attributes);
        }
    }
}
#pragma warning restore
#endif
