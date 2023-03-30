using System;
using System.IO;

using BestHTTP.Extensions;

namespace BestHTTP.Connections
{
    public sealed class BufferedReadNetworkStream : Stream
    {
        #region Network Stats
        public static long TotalNetworkBytesReceived { get => _totalNetworkBytesReceived; }
        private static long _totalNetworkBytesReceived;
        internal static void IncrementTotalNetworkBytesReceived(int amount) => System.Threading.Interlocked.Add(ref _totalNetworkBytesReceived, amount);

        public static long TotalNetworkBytesSent { get => _totalNetworkBytesSent; }
        private static long _totalNetworkBytesSent;

        internal static void IncrementTotalNetworkBytesSent(int amount) => System.Threading.Interlocked.Add(ref _totalNetworkBytesSent, amount);

        public static int TotalConnections { get => _totalConnections; }
        private static int _totalConnections;

        public static int OpenConnections { get => _openConnections; }
        private static int _openConnections;

        internal static void IncrementCurrentConnections()
        {
            System.Threading.Interlocked.Increment(ref _totalConnections);
            System.Threading.Interlocked.Increment(ref _openConnections);
        }
        internal static void DecrementCurrentConnections() => System.Threading.Interlocked.Decrement(ref _openConnections);

        internal static void ResetNetworkStats()
        {
            System.Threading.Interlocked.Exchange(ref _totalNetworkBytesReceived, 0);
            System.Threading.Interlocked.Exchange(ref _totalNetworkBytesSent, 0);
            System.Threading.Interlocked.Exchange(ref _totalConnections, 0);
            System.Threading.Interlocked.Exchange(ref _openConnections, 0);
        }

        #endregion

        public override bool CanRead { get { throw new NotImplementedException(); } }

        public override bool CanSeek { get { throw new NotImplementedException(); } }

        public override bool CanWrite { get { throw new NotImplementedException(); } }

        public override long Length { get { throw new NotImplementedException(); } }

        public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        private ReadOnlyBufferedStream readStream;
        private Stream innerStream;

        public BufferedReadNetworkStream(Stream stream, int bufferSize)
        {
            this.innerStream = stream;
            this.readStream = new ReadOnlyBufferedStream(stream, bufferSize);
            IncrementCurrentConnections();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = this.readStream.Read(buffer, offset, count);
            IncrementTotalNetworkBytesReceived(read);
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            IncrementTotalNetworkBytesSent(count);
            this.innerStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            base.Close();

            if (this.innerStream != null)
            {
                this.innerStream.Close();
                this.innerStream = null;

                DecrementCurrentConnections();
            }
        }
    }

    // Non-used experimental stream. Reading from the inner stream is done parallel and Read is blocked if no data is buffered.
    // Additionally BC reads 5 bytes for the TLS header, than the payload. Buffering data from the network could save at least one context switch per TLS message.
    // In theory it, could help as reading from the network could be done parallel with TLS decryption.
    // However, if decrypting data is done faster than data is coming on the network, waiting for data longer and letting SpinWait to go deep-sleep it's going to
    // resume the thread milliseconds after new data is available. Those little afters are adding up and actually slowing down the download.
    // Not using locking just calling TryDequeue until there's data would solve the slow-down, but with the price of using 100% CPU of a core.
    // The whole struggle might worth it if Unity would implement SocketAsyncEventArgs properly.
    //sealed class BufferedReadNetworkStream : Stream
    //{
    //    public override bool CanRead => throw new NotImplementedException();
    //
    //    public override bool CanSeek => throw new NotImplementedException();
    //
    //    public override bool CanWrite => throw new NotImplementedException();
    //
    //    public override long Length => throw new NotImplementedException();
    //
    //    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    //
    //    byte[] buf;
    //    int available = 0;
    //    int pos = 0;
    //
    //    private System.Net.Sockets.Socket client;
    //    int readBufferSize;
    //    int bufferSize;
    //    private System.Threading.SpinWait spinWait = new System.Threading.SpinWait();
    //
    //    System.Collections.Concurrent.ConcurrentQueue<BufferSegment> downloadedData = new System.Collections.Concurrent.ConcurrentQueue<BufferSegment>();
    //    private int downloadedBytes;
    //    private System.Threading.SpinWait downWait = new System.Threading.SpinWait();
    //    private int closed = 0;
    //
    //    //System.Net.Sockets.SocketAsyncEventArgs socketAsyncEventArgs = new System.Net.Sockets.SocketAsyncEventArgs();
    //
    //    //DateTime started;
    //
    //    public BufferedReadNetworkStream(System.Net.Sockets.Socket socket, int readBufferSize, int bufferSize)
    //    {
    //        this.client = socket;
    //        this.readBufferSize = readBufferSize;
    //        this.bufferSize = bufferSize;
    //
    //        //this.socketAsyncEventArgs.AcceptSocket = this.client;
    //        //
    //        //var buffer = BufferPool.Get(this.readBufferSize, true);
    //        //this.socketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);
    //        //
    //        ////var bufferList = new List<ArraySegment<byte>>();
    //        ////for (int i = 0; i < 1; i++)
    //        ////{
    //        ////    var buffer = BufferPool.Get(this.readBufferSize, true);
    //        ////    bufferList.Add(new ArraySegment<byte>(buffer));
    //        ////}
    //        ////this.socketAsyncEventArgs.BufferList = bufferList;
    //        //
    //        //this.socketAsyncEventArgs.Completed += SocketAsyncEventArgs_Completed;
    //        //
    //        //this.started = DateTime.Now;
    //        //if (!this.client.ReceiveAsync(this.socketAsyncEventArgs))
    //        //    SocketAsyncEventArgs_Completed(null, this.socketAsyncEventArgs);
    //
    //        BestHTTP.PlatformSupport.Threading.ThreadedRunner.RunShortLiving(() =>
    //        {
    //            DateTime started = DateTime.Now;
    //            try
    //            {
    //                while (closed == 0)
    //                {
    //                    var buffer = BufferPool.Get(this.readBufferSize, true);
    //
    //                    int count = this.client.Receive(buffer, 0, buffer.Length, System.Net.Sockets.SocketFlags.None);
    //                    //int count = 0;
    //                    //unsafe {
    //                    //    fixed (byte* pBuffer = buffer)
    //                    //    {
    //                    //        int zero = 0;
    //                    //        count = recvfrom(this.client.Handle, pBuffer, buffer.Length, SocketFlags.None, null, ref zero);
    //                    //    }
    //                    //}
    //
    //                    this.downloadedData.Enqueue(new BufferSegment(buffer, 0, count));
    //                    System.Threading.Interlocked.Add(ref downloadedBytes, count);
    //
    //                    if (HTTPManager.Logger.Level <= Logger.Loglevels.Warning)
    //                        HTTPManager.Logger.Warning(nameof(BufferedReadNetworkStream), $"read count: {count:N0} downloadedBytes: {downloadedBytes:N0} / {this.bufferSize:N0}");
    //
    //                    if (count <= 0)
    //                    {
    //                        System.Threading.Interlocked.Exchange(ref closed, 1);
    //                        return;
    //                    }
    //
    //                    while (downloadedBytes >= this.bufferSize)
    //                    {
    //                        downWait.SpinOnce();
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                UnityEngine.Debug.LogException(ex);
    //            }
    //            finally
    //            {
    //                UnityEngine.Debug.Log($"Reading finished in {(DateTime.Now - started)}");
    //            }
    //        });
    //    }
    //
    //    //private void SocketAsyncEventArgs_Completed(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
    //    //{
    //    //    this.downloadedData.Enqueue(new BufferSegment(e.Buffer, 0, e.BytesTransferred));
    //    //
    //    //    if (e.BytesTransferred == 0)
    //    //    {
    //    //        UnityEngine.Debug.Log($"Reading finished in {(DateTime.Now - started)}");
    //    //        return;
    //    //    }
    //    //
    //    //    int down = System.Threading.Interlocked.Add(ref downloadedBytes, e.BytesTransferred);
    //    //
    //    //    if (HTTPManager.Logger.Level <= Logger.Loglevels.Warning)
    //    //        HTTPManager.Logger.Warning(nameof(BufferedReadNetworkStream), $"SocketAsyncEventArgs_Completed - read count: {e.BytesTransferred:N0} downloadedBytes: {down:N0} / {this.bufferSize:N0}");
    //    //
    //    //    var buffer = BufferPool.Get(this.readBufferSize, true);
    //    //    this.socketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);
    //    //
    //    //    if (!this.client.ReceiveAsync(this.socketAsyncEventArgs))
    //    //        SocketAsyncEventArgs_Completed(null, this.socketAsyncEventArgs);
    //    //}
    //
    //    private void SwitchBuffers(bool waitForData)
    //    {
    //        //HTTPManager.Logger.Error("Read", $"{this.downloadedData.Count}");
    //        BufferSegment segment;
    //        while (!this.downloadedData.TryDequeue(out segment))
    //        {
    //            if (waitForData && closed == 0)
    //            {
    //                if (HTTPManager.Logger.Level <= Logger.Loglevels.Error)
    //                    HTTPManager.Logger.Error(nameof(BufferedReadNetworkStream), $"SpinOnce");
    //                this.spinWait.SpinOnce();
    //            }
    //            else
    //                return;
    //        }
    //
    //        //if (segment.Count <= 0)
    //        //    throw new Exception("Connection closed!");
    //
    //        if (buf != null)
    //            BufferPool.Release(buf);
    //
    //        System.Threading.Interlocked.Add(ref downloadedBytes, -segment.Count);
    //
    //        buf = segment.Data;
    //        available = segment.Count;
    //        pos = 0;
    //    }
    //
    //    public override int Read(byte[] buffer, int offset, int size)
    //    {
    //        if (this.buf == null)
    //        {
    //            SwitchBuffers(true);
    //        }
    //
    //        if (size <= available)
    //        {
    //            Array.Copy(buf, pos, buffer, offset, size);
    //            available -= size;
    //            pos += size;
    //
    //            if (available == 0)
    //            {
    //                SwitchBuffers(false);
    //            }
    //
    //            return size;
    //        }
    //        else
    //        {
    //            int readcount = 0;
    //            if (available > 0)
    //            {
    //                Array.Copy(buf, pos, buffer, offset, available);
    //                offset += available;
    //                readcount += available;
    //                available = 0;
    //                pos = 0;
    //            }
    //
    //            while (true)
    //            {
    //                try
    //                {
    //                    SwitchBuffers(true);
    //                }
    //                catch (Exception ex)
    //                {
    //                    if (readcount > 0)
    //                    {
    //                        return readcount;
    //                    }
    //
    //                    throw (ex);
    //                }
    //
    //                if (available < 1)
    //                {
    //                    if (readcount > 0)
    //                    {
    //                        return readcount;
    //                    }
    //
    //                    return available;
    //                }
    //                else
    //                {
    //                    int toread = size - readcount;
    //                    if (toread <= available)
    //                    {
    //                        Array.Copy(buf, pos, buffer, offset, toread);
    //                        available -= toread;
    //                        pos += toread;
    //                        readcount += toread;
    //                        return readcount;
    //                    }
    //                    else
    //                    {
    //                        Array.Copy(buf, pos, buffer, offset, available);
    //                        offset += available;
    //                        readcount += available;
    //                        pos = 0;
    //                        available = 0;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //
    //    public override long Seek(long offset, SeekOrigin origin)
    //    {
    //        throw new NotImplementedException();
    //    }
    //
    //    public override void SetLength(long value)
    //    {
    //        throw new NotImplementedException();
    //    }
    //
    //    public override void Write(byte[] buffer, int offset, int count)
    //    {
    //        this.client.Send(buffer, offset, count, System.Net.Sockets.SocketFlags.None);
    //
    //        HTTPManager.Logger.Warning(nameof(BufferedReadNetworkStream), $"Wrote: {count}");
    //    }
    //
    //    public override void Close()
    //    {
    //        base.Close();
    //
    //        //socketAsyncEventArgs.Dispose();
    //        //socketAsyncEventArgs = null;
    //    }
    //
    //    public override void Flush()
    //    {
    //    }
    //}
}
