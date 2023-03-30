#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Cms;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Zlib;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Cms
{
    /**
    * Class for reading a CMS Compressed Data stream.
    * <pre>
    *     CMSCompressedDataParser cp = new CMSCompressedDataParser(inputStream);
    *
    *     process(cp.GetContent().GetContentStream());
    * </pre>
    *  Note: this class does not introduce buffering - if you are processing large files you should create
    *  the parser with:
    *  <pre>
    *      CMSCompressedDataParser     ep = new CMSCompressedDataParser(new BufferedInputStream(inputStream, bufSize));
    *  </pre>
    *  where bufSize is a suitably large buffer size.
    */
    public class CmsCompressedDataParser
        : CmsContentInfoParser
    {
        public CmsCompressedDataParser(
            byte[] compressedData)
            : this(new MemoryStream(compressedData, false))
        {
        }

        public CmsCompressedDataParser(
            Stream compressedData)
            : base(compressedData)
        {
        }

		public CmsTypedStream GetContent()
        {
            try
            {
                CompressedDataParser comData = new CompressedDataParser((Asn1SequenceParser)this.contentInfo.GetContent(Asn1Tags.Sequence));
                ContentInfoParser content = comData.GetEncapContentInfo();

                Asn1OctetStringParser bytes = (Asn1OctetStringParser)content.GetContent(Asn1Tags.OctetString);

                return new CmsTypedStream(content.ContentType.ToString(), new ZInputStream(bytes.GetOctetStream()));
            }
            catch (IOException e)
            {
                throw new CmsException("IOException reading compressed content.", e);
            }
        }
    }
}
#pragma warning restore
#endif
