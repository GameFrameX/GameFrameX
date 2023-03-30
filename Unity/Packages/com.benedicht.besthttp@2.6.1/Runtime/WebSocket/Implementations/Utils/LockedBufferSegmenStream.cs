#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2 && !BESTHTTP_DISABLE_WEBSOCKET
using System;
using System.Collections.Generic;
using System.IO;

using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.WebSocket.Implementations.Utils
{
    public sealed class LockedBufferSegmenStream : BufferSegmentStream
    {
        public bool IsClosed { get; private set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (base.bufferList)
            {
                if (this.IsClosed && base.bufferList.Count == 0)
                    return 0;

                int sumReadCount = base.Read(buffer, offset, count);

                return sumReadCount == 0 ? -1 : sumReadCount;
            }
        }

        public override void Write(BufferSegment bufferSegment)
        {
            lock (base.bufferList)
            {
                if (this.IsClosed)
                    return;

                base.Write(bufferSegment);
            }
        }

        public override void Reset()
        {
            lock (base.bufferList)
            {
                base.Reset();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Reset();
        }

        public override void Close()
        {
            this.IsClosed = true;
        }
    }
}
#endif
