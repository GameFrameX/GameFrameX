#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1
{
	public class DerExternalParser
		: Asn1Encodable
	{
		private readonly Asn1StreamParser _parser;


		public DerExternalParser(Asn1StreamParser parser)
		{
			this._parser = parser;
		}

		public IAsn1Convertible ReadObject()
		{
			return _parser.ReadObject();
		}

		public override Asn1Object ToAsn1Object()
		{
            return Parse(_parser);
		}

        internal static DerExternal Parse(Asn1StreamParser sp)
        {
            return new DerExternal(sp.ReadVector());
        }
    }
}
#pragma warning restore
#endif
