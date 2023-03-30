using BestHTTP.PlatformSupport.Memory;
using System;
using System.IO;

namespace BestHTTP.Extensions
{
    /// <summary>
    /// A custom buffer stream implementation that will not close the underlying stream.
    /// </summary>
    public sealed class WriteOnlyBufferedStream : Stream
    {
        public override bool CanRead { get { return false; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { return this.buffer.Length; } }

        public override long Position { get { return this._position; } set { throw new NotImplementedException("Position set"); } }
        private int _position;

        private byte[] buffer;
        private Stream stream;

        public WriteOnlyBufferedStream(Stream stream, int bufferSize)
        {
            this.stream = stream;

            this.buffer = BufferPool.Get(bufferSize, true);
            this._position = 0;
        }

        public override void Flush()
        {
            if (this._position > 0)
            {
                this.stream.Write(this.buffer, 0, this._position);
                this.stream.Flush();

                //if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                //    HTTPManager.Logger.Information("WriteOnlyBufferedStream", string.Format("Flushed {0:N0} bytes", this._position));

                this._position = 0;
            }
        }

        public override void Write(byte[] bufferFrom, int offset, int count)
        {
            while (count > 0)
            {
                int writeCount = Math.Min(count, this.buffer.Length - this._position);
                Array.Copy(bufferFrom, offset, this.buffer, this._position, writeCount);
            
                this._position += writeCount;
                offset += writeCount;
                count -= writeCount;
            
                if (this._position == this.buffer.Length)
                    this.Flush();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0;
        }

        public override void SetLength(long value) { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && this.buffer != null)
                BufferPool.Release(this.buffer);
            this.buffer = null;
        }
    }
}
