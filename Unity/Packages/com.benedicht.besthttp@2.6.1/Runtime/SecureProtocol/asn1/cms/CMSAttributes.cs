#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Pkcs;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Cms
{
    public abstract class CmsAttributes
    {
        public static readonly DerObjectIdentifier ContentType		= PkcsObjectIdentifiers.Pkcs9AtContentType;
        public static readonly DerObjectIdentifier MessageDigest	= PkcsObjectIdentifiers.Pkcs9AtMessageDigest;
        public static readonly DerObjectIdentifier SigningTime		= PkcsObjectIdentifiers.Pkcs9AtSigningTime;
		public static readonly DerObjectIdentifier CounterSignature = PkcsObjectIdentifiers.Pkcs9AtCounterSignature;
		public static readonly DerObjectIdentifier ContentHint		= PkcsObjectIdentifiers.IdAAContentHint;
	}
}
#pragma warning restore
#endif
