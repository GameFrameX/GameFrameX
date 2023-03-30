#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;
#if UNITY_WSA && !UNITY_EDITOR && !ENABLE_IL2CPP
using System.TypeFix;
#endif

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.X509.Store
{
	public sealed class X509StoreFactory
	{
		private X509StoreFactory()
		{
		}

		public static IX509Store Create(
			string					type,
			IX509StoreParameters	parameters)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			string[] parts = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.ToUpperInvariant(type).Split('/');

            if (parts.Length < 2)
				throw new ArgumentException("type");

			if (parts[1] != "COLLECTION")
				throw new NoSuchStoreException("X.509 store type '" + type + "' not available.");

			X509CollectionStoreParameters p = (X509CollectionStoreParameters) parameters;
			ICollection coll = p.GetCollection();

			switch (parts[0])
			{
				case "ATTRIBUTECERTIFICATE":
					checkCorrectType(coll, typeof(IX509AttributeCertificate));
					break;
				case "CERTIFICATE":
					checkCorrectType(coll, typeof(X509Certificate));
					break;
				case "CERTIFICATEPAIR":
					checkCorrectType(coll, typeof(X509CertificatePair));
					break;
				case "CRL":
					checkCorrectType(coll, typeof(X509Crl));
					break;
				default:
					throw new NoSuchStoreException("X.509 store type '" + type + "' not available.");
			}

			return new X509CollectionStore(coll);
		}

		private static void checkCorrectType(ICollection coll, Type t)
		{
			foreach (object o in coll)
			{
				if (!t.IsInstanceOfType(o))
					throw new InvalidCastException("Can't cast object to type: " + t.FullName);
			}
		}
	}
}
#pragma warning restore
#endif
