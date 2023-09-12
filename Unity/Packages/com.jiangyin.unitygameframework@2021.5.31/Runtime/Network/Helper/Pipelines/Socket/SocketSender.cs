using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace UnityGameFramework.Network.Pipelines
{
    internal class SocketSender
    {
        private readonly System.Net.Sockets.Socket _socket;
        private readonly SocketAsyncEventArgs _eventArgs = new SocketAsyncEventArgs();
        private readonly SocketAwaitable _awaitable;

        private List<ArraySegment<byte>> _bufferList;

        public SocketSender(System.Net.Sockets.Socket socket, PipeScheduler scheduler)
        {
            _socket = socket;
            _awaitable = new SocketAwaitable(scheduler);
            _eventArgs.UserToken = _awaitable;
            _eventArgs.Completed += (_, e) => ((SocketAwaitable) e.UserToken).Complete(e.BytesTransferred, e.SocketError);
        }


        public SocketAwaitable SendAsync(in ReadOnlySequence<byte> buffers)
        {
            {
                if (buffers.IsSingleSegment)
                    return SendAsync(buffers.First);
            }

#if NETCOREAPP
            if (!_eventArgs.MemoryBuffer.Equals(Memory<byte>.Empty))
#else
            if (_eventArgs.Buffer != null)
#endif
            {
                _eventArgs.SetBuffer(null, 0, 0);
            }

            _eventArgs.BufferList = GetBufferList(buffers);

            if (!_socket.SendAsync(_eventArgs))
            {
                _awaitable.Complete(_eventArgs.BytesTransferred, _eventArgs.SocketError);
            }

            return _awaitable;
        }

        private SocketAwaitable SendAsync(ReadOnlyMemory<byte> memory)
        {
            // The BufferList getter is much less expensive then the setter.
            if (_eventArgs.BufferList != null)
            {
                _eventArgs.BufferList = null;
            }


            var segment = memory.GetArray();

            _eventArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            if (!_socket.SendAsync(_eventArgs))
            {
                _awaitable.Complete(_eventArgs.BytesTransferred, _eventArgs.SocketError);
            }

            return _awaitable;
        }

        private List<ArraySegment<byte>> GetBufferList(in ReadOnlySequence<byte> buffer)
        {
            Debug.Assert(!buffer.IsEmpty);
            Debug.Assert(!buffer.IsSingleSegment);

            if (_bufferList == null)
            {
                _bufferList = new List<ArraySegment<byte>>();
            }
            else
            {
                // Buffers are pooled, so it's OK to root them until the next multi-buffer write.
                _bufferList.Clear();
            }

            foreach (var b in buffer)
            {
                if (!MemoryMarshal.TryGetArray(b, out var result))
                {
                    throw new InvalidOperationException("Buffer backed by array was expected");
                }

                _bufferList.Add(result);
            }

            return _bufferList;
        }
    }
}