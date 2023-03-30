#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Collections;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Pkcs
{
    public abstract class Pkcs12Entry
    {
        private readonly IDictionary attributes;

		protected internal Pkcs12Entry(
            IDictionary attributes)
        {
            this.attributes = attributes;

			foreach (DictionaryEntry entry in attributes)
			{
				if (!(entry.Key is string))
					throw new ArgumentException("Attribute keys must be of type: " + typeof(string).FullName, "attributes");
				if (!(entry.Value is Asn1Encodable))
					throw new ArgumentException("Attribute values must be of type: " + typeof(Asn1Encodable).FullName, "attributes");
			}
        }


		public Asn1Encodable GetBagAttribute(
            DerObjectIdentifier oid)
        {
            return (Asn1Encodable)this.attributes[oid.Id];
        }


		public Asn1Encodable GetBagAttribute(
            string oid)
        {
            return (Asn1Encodable)this.attributes[oid];
        }


        public IEnumerator GetBagAttributeKeys()
        {
            return this.attributes.Keys.GetEnumerator();
        }

		public Asn1Encodable this[
			DerObjectIdentifier oid]
		{
			get { return (Asn1Encodable) this.attributes[oid.Id]; }
		}

		public Asn1Encodable this[
			string oid]
		{
			get { return (Asn1Encodable) this.attributes[oid]; }
		}

		public IEnumerable BagAttributeKeys
		{
			get { return new EnumerableProxy(this.attributes.Keys); }
		}
    }
}
#pragma warning restore
#endif
