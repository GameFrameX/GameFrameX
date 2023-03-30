using System;
using System.Collections.Generic;
using System.IO;

using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.Extensions
{
    public class BufferSegmentStream : Stream
    {
        public override bool CanRead { get { return false; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return false; } }

        public override long Length { get { return this._length; } }
        protected long _length;

        public override long Position { get { return 0; } set { } }

        protected List<BufferSegment> bufferList = new List<BufferSegment>();

        private byte[] _tempByteArray = new byte[1];

        public override int ReadByte()
        {
            if (Read(this._tempByteArray, 0, 1) == 0)
                return -1;

            return this._tempByteArray[0];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int sumReadCount = 0;

            while (count > 0 && bufferList.Count > 0)
            {
                BufferSegment buff = this.bufferList[0];

                int readCount = Math.Min(count, buff.Count);

                Array.Copy(buff.Data, buff.Offset, buffer, offset, readCount);

                sumReadCount += readCount;
                offset += readCount;
                count -= readCount;

                if (readCount >= buff.Count)
                {
                    this.bufferList.RemoveAt(0);
                    BufferPool.Release(buff.Data);
                }
                else
                    this.bufferList[0] = new BufferSegment(buff.Data, buff.Offset + readCount, buff.Count - readCount);
            }

            this._length -= sumReadCount;

            return sumReadCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(new BufferSegment(buffer, offset, count));
        }

        public virtual void Write(BufferSegment bufferSegment)
        {
            this.bufferList.Add(bufferSegment);
            this._length += bufferSegment.Count;
        }

        public virtual void Reset()
        {
            for (int i = 0; i < this.bufferList.Count; ++i)
                BufferPool.Release(this.bufferList[i]);
            this.bufferList.Clear();
            this._length = 0;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this._length = 0;
        }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
