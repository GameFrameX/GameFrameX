#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Misc
{
    public class NetscapeRevocationUrl
        : DerIA5String
    {
        public NetscapeRevocationUrl(DerIA5String str)
			: base(str.GetString())
        {
        }

        public override string ToString()
        {
            return "NetscapeRevocationUrl: " + this.GetString();
        }
    }
}
#pragma warning restore
#endif
