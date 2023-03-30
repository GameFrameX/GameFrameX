#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1
{
	public class DerSetGenerator
		: DerGenerator
	{
		private readonly MemoryStream _bOut = new MemoryStream();

		public DerSetGenerator(
			Stream outStream)
			: base(outStream)
		{
		}

		public DerSetGenerator(
			Stream	outStream,
			int		tagNo,
			bool	isExplicit)
			: base(outStream, tagNo, isExplicit)
		{
		}

		public override void AddObject(Asn1Encodable obj)
		{
            obj.EncodeTo(_bOut, Asn1Encodable.Der);
		}

        public override void AddObject(Asn1Object obj)
        {
            obj.EncodeTo(_bOut, Asn1Encodable.Der);
        }

        public override Stream GetRawOutputStream()
		{
			return _bOut;
		}

		public override void Close()
		{
			WriteDerEncoded(Asn1Tags.Constructed | Asn1Tags.Set, _bOut.ToArray());
		}
	}
}
#pragma warning restore
#endif
