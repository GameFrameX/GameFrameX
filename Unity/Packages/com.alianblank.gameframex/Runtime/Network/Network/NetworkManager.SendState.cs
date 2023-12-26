//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.IO;

namespace GameFrameX.Network
{
    public sealed partial class NetworkManager
    {
        public sealed class SendState : IDisposable
        {
            private const int DefaultBufferLength = 1024 * 64;
            private MemoryStream _stream;
            private bool _disposed;

            public SendState()
            {
                _stream = new MemoryStream(DefaultBufferLength);
                _disposed = false;
            }

            public MemoryStream Stream
            {
                get { return _stream; }
            }

            public void Reset()
            {
                _stream.Position = 0L;
                _stream.SetLength(0L);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    if (_stream != null)
                    {
                        _stream.Dispose();
                        _stream = null;
                    }
                }

                _disposed = true;
            }
        }
    }
}