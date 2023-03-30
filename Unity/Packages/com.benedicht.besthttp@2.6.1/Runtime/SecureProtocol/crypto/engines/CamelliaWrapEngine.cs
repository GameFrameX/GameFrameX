#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Engines
{
	/// <remarks>
	/// An implementation of the Camellia key wrapper based on RFC 3657/RFC 3394.
	/// <p/>
	/// For further details see: <a href="http://www.ietf.org/rfc/rfc3657.txt">http://www.ietf.org/rfc/rfc3657.txt</a>.
	/// </remarks>
	public class CamelliaWrapEngine
		: Rfc3394WrapEngine
	{
		public CamelliaWrapEngine()
			: base(new CamelliaEngine())
		{
		}
	}
}
#pragma warning restore
#endif
