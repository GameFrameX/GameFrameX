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
        public sealed class ReceiveState : IDisposable
        {
            private const int DefaultBufferLength = 1024 * 64;
            private MemoryStream _mStream;
            private IPacketReceiveHeaderHandler _packetReceiveHeaderHandler;
            private IPacketReceiveBodyHandler _packetReceiveBodyHandler;
            private bool _disposed;

            public ReceiveState()
            {
                _mStream = new MemoryStream(DefaultBufferLength);
                _packetReceiveHeaderHandler = null;
                _packetReceiveBodyHandler = null;
                _disposed = false;
            }

            public MemoryStream Stream
            {
                get { return _mStream; }
            }

            public IPacketReceiveHeaderHandler PacketHeaderHandler
            {
                get { return _packetReceiveHeaderHandler; }
            }

            public IPacketReceiveBodyHandler PacketBodyHandler
            {
                get { return _packetReceiveBodyHandler; }
            }

            public void PrepareForPacketHeader(int packetHeaderLength)
            {
                Reset(packetHeaderLength, null, null);
            }

            public void PrepareForPacket(IPacketReceiveHeaderHandler packetHeader, IPacketReceiveBodyHandler packetBody)
            {
                if (packetHeader == null)
                {
                    throw new ArgumentNullException(nameof(packetHeader), "Packet header is invalid.");
                }

                if (packetBody == null)
                {
                    throw new ArgumentNullException(nameof(packetBody), "Packet body is invalid.");
                }

                Reset(packetHeader.PacketLength, packetHeader, packetBody);
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
                    if (_mStream != null)
                    {
                        _mStream.Dispose();
                        _mStream = null;
                    }
                }

                _disposed = true;
            }

            private void Reset(int targetLength, IPacketReceiveHeaderHandler packetReceiveHeaderHandler, IPacketReceiveBodyHandler packetReceiveBodyHandler)
            {
                if (targetLength < 0)
                {
                    throw new GameFrameworkException("Target length is invalid.");
                }

                _mStream.Position = 0L;
                _mStream.SetLength(targetLength);
                _packetReceiveHeaderHandler = packetReceiveHeaderHandler;
                _packetReceiveBodyHandler = packetReceiveBodyHandler;
            }
        }
    }
}