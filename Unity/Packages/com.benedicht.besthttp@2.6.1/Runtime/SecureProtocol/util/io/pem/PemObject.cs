#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Collections;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO.Pem
{
	public class PemObject
		: PemObjectGenerator
	{
		private string		type;
		private IList		headers;
		private byte[]		content;

		public PemObject(string type, byte[] content)
			: this(type, BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateArrayList(), content)
		{
		}

		public PemObject(String type, IList headers, byte[] content)
		{
			this.type = type;
            this.headers = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateArrayList(headers);
			this.content = content;
		}

		public string Type
		{
			get { return type; }
		}

		public IList Headers
		{
			get { return headers; }
		}

		public byte[] Content
		{
			get { return content; }
		}

		public PemObject Generate()
		{
			return this;
		}
	}
}
#pragma warning restore
#endif
