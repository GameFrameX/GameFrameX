#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Cms
{
    public class AttributeTable
    {
        private readonly IDictionary attributes;

#if !(SILVERLIGHT || PORTABLE || NETFX_CORE)
        [Obsolete]
        public AttributeTable(
            Hashtable attrs)
        {
            this.attributes = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable(attrs);
        }
#endif

        public AttributeTable(
            IDictionary attrs)
        {
            this.attributes = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable(attrs);
        }

        public AttributeTable(
            Asn1EncodableVector v)
        {
            this.attributes = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable(v.Count);

			foreach (Asn1Encodable o in v)
            {
                Attribute a = Attribute.GetInstance(o);

				AddAttribute(a);
            }
        }

        public AttributeTable(
            Asn1Set s)
        {
            this.attributes = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable(s.Count);

			for (int i = 0; i != s.Count; i++)
            {
                Attribute a = Attribute.GetInstance(s[i]);

                AddAttribute(a);
            }
        }

		public AttributeTable(
			Attributes attrs)
			: this(Asn1Set.GetInstance(attrs.ToAsn1Object()))
		{
		}

		private void AddAttribute(
            Attribute a)
        {
			DerObjectIdentifier oid = a.AttrType;
            object obj = attributes[oid];

            if (obj == null)
            {
                attributes[oid] = a;
            }
            else
            {
                IList v;

                if (obj is Attribute)
                {
                    v = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateArrayList();

                    v.Add(obj);
                    v.Add(a);
                }
                else
                {
                    v = (IList) obj;

                    v.Add(a);
                }

                attributes[oid] = v;
            }
        }

		/// <summary>Return the first attribute matching the given OBJECT IDENTIFIER</summary>
		public Attribute this[DerObjectIdentifier oid]
		{
			get
			{
				object obj = attributes[oid];

				if (obj is IList)
				{
					return (Attribute)((IList)obj)[0];
				}

				return (Attribute) obj;
			}
		}


        public Attribute Get(
            DerObjectIdentifier oid)
        {
			return this[oid];
        }

		/**
        * Return all the attributes matching the OBJECT IDENTIFIER oid. The vector will be
        * empty if there are no attributes of the required type present.
        *
        * @param oid type of attribute required.
        * @return a vector of all the attributes found of type oid.
        */
        public Asn1EncodableVector GetAll(
            DerObjectIdentifier oid)
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

            object obj = attributes[oid];

			if (obj is IList)
            {
                foreach (Attribute a in (IList)obj)
                {
                    v.Add(a);
                }
            }
            else if (obj != null)
            {
                v.Add((Attribute) obj);
            }

			return v;
        }

		public int Count
		{
			get
			{
				int total = 0;

				foreach (object o in attributes.Values)
				{
					if (o is IList)
					{
						total += ((IList)o).Count;
					}
					else
					{
						++total;
					}
				}

				return total;
			}
		}

        public IDictionary ToDictionary()
        {
            return BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateHashtable(attributes);
        }

#if !(SILVERLIGHT || PORTABLE || NETFX_CORE)

		public Hashtable ToHashtable()
        {
            return new Hashtable(attributes);
        }
#endif

		public Asn1EncodableVector ToAsn1EncodableVector()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			foreach (object obj in attributes.Values)
            {
                if (obj is IList)
                {
                    foreach (object el in (IList)obj)
                    {
                        v.Add(Attribute.GetInstance(el));
                    }
                }
                else
                {
                    v.Add(Attribute.GetInstance(obj));
                }
            }

			return v;
        }

		public Attributes ToAttributes()
		{
			return new Attributes(this.ToAsn1EncodableVector());
		}

		/**
		 * Return a new table with the passed in attribute added.
		 *
		 * @param attrType
		 * @param attrValue
		 * @return
		 */
		public AttributeTable Add(DerObjectIdentifier attrType, Asn1Encodable attrValue)
		{
			AttributeTable newTable = new AttributeTable(attributes);

			newTable.AddAttribute(new Attribute(attrType, new DerSet(attrValue)));

			return newTable;
		}

		public AttributeTable Remove(DerObjectIdentifier attrType)
		{
			AttributeTable newTable = new AttributeTable(attributes);

			newTable.attributes.Remove(attrType);

			return newTable;
		}
    }
}
#pragma warning restore
#endif
