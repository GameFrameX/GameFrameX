#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Cms
{
	public class CmsProcessableInputStream
		: CmsProcessable, CmsReadable
	{
		private readonly Stream input;

        private bool used = false;

        public CmsProcessableInputStream(Stream input)
		{
			this.input = input;
		}

        public virtual Stream GetInputStream()
		{
			CheckSingleUsage();

            return input;
		}

        public virtual void Write(Stream output)
		{
			CheckSingleUsage();

			Streams.PipeAll(input, output);
            BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.Dispose(input);
		}

        [Obsolete]
		public virtual object GetContent()
		{
			return GetInputStream();
		}

        protected virtual void CheckSingleUsage()
		{
			lock (this)
			{
				if (used)
					throw new InvalidOperationException("CmsProcessableInputStream can only be used once");

                used = true;
			}
		}
	}
}
#pragma warning restore
#endif
