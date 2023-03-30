#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1
{
	public interface IAsn1Convertible
	{
		Asn1Object ToAsn1Object();
	}
}
#pragma warning restore
#endif
