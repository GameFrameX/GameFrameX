#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1
{

    public class BerOutputStream
        : DerOutputStream
    {

        public BerOutputStream(Stream os)
            : base(os)
        {
        }

        public override void WriteObject(Asn1Encodable encodable)
        {
            Asn1OutputStream.Create(s).WriteObject(encodable);
        }

        public override void WriteObject(Asn1Object primitive)
        {
            Asn1OutputStream.Create(s).WriteObject(primitive);
        }
    }
}
#pragma warning restore
#endif
