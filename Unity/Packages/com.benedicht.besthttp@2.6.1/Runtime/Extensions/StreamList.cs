using BestHTTP.PlatformSupport.Memory;
using System;

namespace BestHTTP.Extensions
{
    /// <summary>
    /// Wrapper of multiple streams. Writes and reads are both supported. Read goes trough all the streams.
    /// </summary>
    public sealed class StreamList : System.IO.Stream
    {
        private System.IO.Stream[] Streams;
        private int CurrentIdx;

        public StreamList(params System.IO.Stream[] streams)
        {
            this.Streams = streams;
            this.CurrentIdx = 0;
        }

        public override bool CanRead
        {
            get
            {
                if (CurrentIdx >= Streams.Length)
                    return false;
                return Streams[CurrentIdx].CanRead;
            }
        }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite
        {
            get
            {
                if (CurrentIdx >= Streams.Length)
                    return false;
                return Streams[CurrentIdx].CanWrite;
            }
        }

        public override void Flush()
        {
            if (CurrentIdx >= Streams.Length)
                return;

            // We have to call the flush to all previous streams, as we may advanced the CurrentIdx
            for (int i = 0; i <= CurrentIdx; ++i)
                Streams[i].Flush();
        }

        public override long Length
        {
            get
            {
                if (CurrentIdx >= Streams.Length)
                    return 0;

                long length = 0;
                for (int i = 0; i < Streams.Length; ++i)
                    length += Streams[i].Length;

                return length;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (CurrentIdx >= Streams.Length)
                return -1;

            int readCount = Streams[CurrentIdx].Read(buffer, offset, count);

            while (readCount < count && ++CurrentIdx < Streams.Length)
            {
                // Dispose previous stream
                try
                {
                    Streams[CurrentIdx - 1].Dispose();
                    Streams[CurrentIdx - 1] = null;
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("StreamList", "Dispose", ex);
                }

                readCount += Streams[CurrentIdx].Read(buffer, offset + readCount, count - readCount);
            }

            return readCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CurrentIdx >= Streams.Length)
                return;

            Streams[CurrentIdx].Write(buffer, offset, count);
        }

        public void Write(string str)
        {
            byte[] bytes = str.GetASCIIBytes();

            this.Write(bytes, 0, bytes.Length);
            BufferPool.Release(bytes);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 0; i < Streams.Length; ++i)
                    if (Streams[i] != null)
                    {
                        try
                        {
                            Streams[i].Dispose();
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("StreamList", "Dispose", ex);
                        }
                    }
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException("Position get");
            }
            set
            {
                throw new NotImplementedException("Position set");
            }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            if (CurrentIdx >= Streams.Length)
                return 0;

            return Streams[CurrentIdx].Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException("SetLength");
        }
    }
}
