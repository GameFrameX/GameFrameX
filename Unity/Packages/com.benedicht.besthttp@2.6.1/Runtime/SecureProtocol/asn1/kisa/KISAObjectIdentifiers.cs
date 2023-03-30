#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Kisa
{
	public abstract class KisaObjectIdentifiers
	{
		public static readonly DerObjectIdentifier IdSeedCbc = new DerObjectIdentifier("1.2.410.200004.1.4");
		public static readonly DerObjectIdentifier IdNpkiAppCmsSeedWrap = new DerObjectIdentifier("1.2.410.200004.7.1.1.1");
	}
}
#pragma warning restore
#endif
