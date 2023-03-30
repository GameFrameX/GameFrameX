#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tls
{
    /// <summary>OutputStream based on a ByteQueue implementation.</summary>
    public sealed class ByteQueueOutputStream
        : BaseOutputStream
    {
        private readonly ByteQueue m_buffer;

        public ByteQueueOutputStream()
        {
            this.m_buffer = new ByteQueue();
        }

        public ByteQueue Buffer
        {
            get { return m_buffer; }
        }

        public override void WriteByte(byte b)
        {
            m_buffer.AddData(new byte[]{ b }, 0, 1);
        }

        public override void Write(byte[] buf, int off, int len)
        {
            m_buffer.AddData(buf, off, len);
        }
    }
}
#pragma warning restore
#endif
