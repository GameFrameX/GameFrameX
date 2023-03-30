#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using BestHTTP.PlatformSupport.Memory;
using System;
using System.Collections.Generic;
using System.IO;

namespace BestHTTP.Connections.HTTP2
{
    interface IFrameDataView : IDisposable
    {
        long Length { get; }
        long Position { get; }

        void AddFrame(HTTP2FrameHeaderAndPayload frame);
        int ReadByte();
        int Read(byte[] buffer, int offset, int count);
    }

    abstract class CommonFrameView : IFrameDataView
    {
        public long Length { get; protected set; }
        public long Position { get; protected set; }

        protected List<HTTP2FrameHeaderAndPayload> frames = new List<HTTP2FrameHeaderAndPayload>();
        protected int currentFrameIdx = -1;
        protected byte[] data;
        protected UInt32 dataOffset;
        protected UInt32 maxOffset;

        public abstract void AddFrame(HTTP2FrameHeaderAndPayload frame);
        protected abstract long CalculateDataLengthForFrame(HTTP2FrameHeaderAndPayload frame);

        public virtual int Read(byte[] buffer, int offset, int count)
        {
            if (this.dataOffset >= this.maxOffset && !AdvanceFrame())
                return -1;

            int readCount = 0;

            while (count > 0)
            {
                long copyCount = Math.Min(count, this.maxOffset - this.dataOffset);

                Array.Copy(this.data, this.dataOffset, buffer, offset + readCount, copyCount);

                count -= (int)copyCount;
                readCount += (int)copyCount;

                this.dataOffset += (UInt32)copyCount;
                this.Position += copyCount;

                if (this.dataOffset >= this.maxOffset && !AdvanceFrame())
                    break;
            }

            return readCount;
        }

        public virtual int ReadByte()
        {
            if (this.dataOffset >= this.maxOffset && !AdvanceFrame())
                return -1;

            byte data = this.data[this.dataOffset];
            this.dataOffset++;
            this.Position++;

            return data;
        }

        protected abstract bool AdvanceFrame();

        public virtual void Dispose()
        {
            for (int i = 0; i < this.frames.Count; ++i)
                if (this.frames[i].Payload != null && !this.frames[i].DontUseMemPool)
                    BufferPool.Release(this.frames[i].Payload);
            this.frames.Clear();
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder("[CommonFrameView ");

            for (int i = 0; i < this.frames.Count; ++i) {
                sb.AppendFormat("{0} Payload: {1}\n", this.frames[i], this.frames[i].PayloadAsHex());
            }

            sb.Append("]");

            return sb.ToString();
        }
    }

    sealed class HeaderFrameView : CommonFrameView
    {
        public override void AddFrame(HTTP2FrameHeaderAndPayload frame)
        {
            if (frame.Type != HTTP2FrameTypes.HEADERS && frame.Type != HTTP2FrameTypes.CONTINUATION)
                throw new ArgumentException("HeaderFrameView - Unexpected frame type: " + frame.Type);

            this.frames.Add(frame);
            this.Length += CalculateDataLengthForFrame(frame);

            if (this.currentFrameIdx == -1)
                AdvanceFrame();
        }

        protected override long CalculateDataLengthForFrame(HTTP2FrameHeaderAndPayload frame)
        {
            switch (frame.Type)
            {
                case HTTP2FrameTypes.HEADERS:
                    return HTTP2FrameHelper.ReadHeadersFrame(frame).HeaderBlockFragmentLength;

                case HTTP2FrameTypes.CONTINUATION:
                    return frame.PayloadLength;
            }

            return 0;
        }

        protected override bool AdvanceFrame()
        {
            if (this.currentFrameIdx >= this.frames.Count - 1)
                return false;

            this.currentFrameIdx++;
            HTTP2FrameHeaderAndPayload frame = this.frames[this.currentFrameIdx];

            this.data = frame.Payload;

            switch (frame.Type)
            {
                case HTTP2FrameTypes.HEADERS:
                    var header = HTTP2FrameHelper.ReadHeadersFrame(frame);
                    this.dataOffset = header.HeaderBlockFragmentIdx;
                    this.maxOffset = this.dataOffset + header.HeaderBlockFragmentLength;
                    break;

                case HTTP2FrameTypes.CONTINUATION:
                    this.dataOffset = 0;
                    this.maxOffset = frame.PayloadLength;
                    break;
            }

            return true;
        }
    }

    sealed class DataFrameView : CommonFrameView
    {
        public override void AddFrame(HTTP2FrameHeaderAndPayload frame)
        {
            if (frame.Type != HTTP2FrameTypes.DATA)
                throw new ArgumentException("HeaderFrameView - Unexpected frame type: " + frame.Type);

            this.frames.Add(frame);
            this.Length += CalculateDataLengthForFrame(frame);
        }

        protected override long CalculateDataLengthForFrame(HTTP2FrameHeaderAndPayload frame)
        {
            return HTTP2FrameHelper.ReadDataFrame(frame).DataLength;
        }

        protected override bool AdvanceFrame()
        {
            if (this.currentFrameIdx >= this.frames.Count - 1)
                return false;

            this.currentFrameIdx++;
            HTTP2FrameHeaderAndPayload frame = this.frames[this.currentFrameIdx];
            HTTP2DataFrame dataFrame = HTTP2FrameHelper.ReadDataFrame(frame);

            this.data = frame.Payload;
            this.dataOffset = dataFrame.DataIdx;
            this.maxOffset = dataFrame.DataIdx + dataFrame.DataLength;

            return true;
        }
    }

    sealed class FramesAsStreamView : Stream
    {
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }
        public override long Length { get { return this.view.Length; } }
        public override long Position { get { return this.view.Position; } set { throw new NotSupportedException(); } }

        private IFrameDataView view;

        public FramesAsStreamView(IFrameDataView view)
        {
            this.view = view;
        }

        public void AddFrame(HTTP2FrameHeaderAndPayload frame)
        {
            this.view.AddFrame(frame);
        }

        public override int ReadByte()
        {
            return this.view.ReadByte();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.view.Read(buffer, offset, count);
        }

        public override void Close()
        {
            base.Close();
            this.view.Dispose();
        }

        public override void Flush() {}
        public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
        public override void SetLength(long value) { throw new NotImplementedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }

        public override string ToString()
        {
            return this.view.ToString();
        }
    }
}

#endif
