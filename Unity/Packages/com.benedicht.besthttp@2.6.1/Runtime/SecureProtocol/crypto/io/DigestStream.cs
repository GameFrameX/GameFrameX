#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.IO
{
	public class DigestStream
		: Stream
	{
		protected readonly Stream stream;
		protected readonly IDigest inDigest;
		protected readonly IDigest outDigest;

		public DigestStream(
			Stream	stream,
			IDigest	readDigest,
			IDigest	writeDigest)
		{
			this.stream = stream;
			this.inDigest = readDigest;
			this.outDigest = writeDigest;
		}

		public virtual IDigest ReadDigest()
		{
			return inDigest;
		}

		public virtual IDigest WriteDigest()
		{
			return outDigest;
		}

		public override int Read(
			byte[]	buffer,
			int		offset,
			int		count)
		{
			int n = stream.Read(buffer, offset, count);
			if (inDigest != null)
			{
				if (n > 0)
				{
					inDigest.BlockUpdate(buffer, offset, n);
				}
			}
			return n;
		}

		public override int ReadByte()
		{
			int b = stream.ReadByte();
			if (inDigest != null)
			{
				if (b >= 0)
				{
					inDigest.Update((byte)b);
				}
			}
			return b;
		}

		public override void Write(
			byte[]	buffer,
			int		offset,
			int		count)
		{
			if (outDigest != null)
			{
				if (count > 0)
				{
					outDigest.BlockUpdate(buffer, offset, count);
				}
			}
			stream.Write(buffer, offset, count);
		}

		public override void WriteByte(
			byte b)
		{
			if (outDigest != null)
			{
				outDigest.Update(b);
			}
			stream.WriteByte(b);
		}

		public override bool CanRead
		{
			get { return stream.CanRead; }
		}

		public override bool CanWrite
		{
			get { return stream.CanWrite; }
		}

		public override bool CanSeek
		{
			get { return stream.CanSeek; }
		}

		public override long Length
		{
			get { return stream.Length; }
		}

		public override long Position
		{
			get { return stream.Position; }
			set { stream.Position = value; }
		}

#if PORTABLE || NETFX_CORE
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.Dispose(stream);
            }
            base.Dispose(disposing);
        }
#else
		public override void Close()
		{
            BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.Dispose(stream);
            base.Close();
		}
#endif

        public override void Flush()
		{
			stream.Flush();
		}

		public override long Seek(
			long		offset,
			SeekOrigin	origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(
			long length)
		{
			stream.SetLength(length);
		}
	}
}

#pragma warning restore
#endif
