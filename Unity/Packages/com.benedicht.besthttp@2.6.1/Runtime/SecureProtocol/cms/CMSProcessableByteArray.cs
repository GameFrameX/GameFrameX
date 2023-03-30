#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Cms;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Cms
{
	/**
	* a holding class for a byte array of data to be processed.
	*/
	public class CmsProcessableByteArray
		: CmsProcessable, CmsReadable
	{
	    private readonly DerObjectIdentifier type;
		private readonly byte[] bytes;

        public CmsProcessableByteArray(byte[] bytes)
        {
            type = CmsObjectIdentifiers.Data;
			this.bytes = bytes;
		}

	    public CmsProcessableByteArray(DerObjectIdentifier type, byte[] bytes)
	    {
	        this.bytes = bytes;
	        this.type = type;
	    }

	    public DerObjectIdentifier Type
	    {
	        get { return type; }
	    }

        public virtual Stream GetInputStream()
		{
			return new MemoryStream(bytes, false);
		}

        public virtual void Write(Stream zOut)
		{
			zOut.Write(bytes, 0, bytes.Length);
		}

        /// <returns>A clone of the byte array</returns>
        [Obsolete]
		public virtual object GetContent()
		{
			return bytes.Clone();
		}
	}
}
#pragma warning restore
#endif
