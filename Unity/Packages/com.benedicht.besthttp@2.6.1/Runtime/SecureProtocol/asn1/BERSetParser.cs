#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1
{
    public class BerSetParser
        : Asn1SetParser
    {
        private readonly Asn1StreamParser _parser;

        internal BerSetParser(Asn1StreamParser parser)
        {
            this._parser = parser;
        }

        public IAsn1Convertible ReadObject()
        {
            return _parser.ReadObject();
        }

        public Asn1Object ToAsn1Object()
        {
            return Parse(_parser);
        }

        internal static BerSet Parse(Asn1StreamParser sp)
        {
            return new BerSet(sp.ReadVector());
        }
    }
}
#pragma warning restore
#endif
