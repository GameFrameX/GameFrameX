#if UNITY_WSA && !UNITY_EDITOR && !ENABLE_IL2CPP

using System;
using System.IO;

using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace BestHTTP.PlatformSupport.TcpClient.WinRT
{
    public sealed class DataReaderWriterStream : System.IO.Stream
    {
        private TcpClient Client { get; set; }

        public DataReaderWriterStream(TcpClient socket)
        {
            this.Client = socket;
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override void Flush() { }
        public override long Length { get { throw new NotImplementedException(); } }
        public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public bool DataAvailable { get { return true; } }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Windows.Storage.Streams.Buffer tmpBuffer = new Windows.Storage.Streams.Buffer((uint)count);

            try
            {
                var task = Client.Socket.InputStream.ReadAsync(tmpBuffer, tmpBuffer.Capacity, InputStreamOptions.Partial)
                            .AsTask();
                task.ConfigureAwait(false);
                task.Wait();
            }
            catch(AggregateException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                else
                    throw ex;
            }

            DataReader reader = DataReader.FromBuffer(tmpBuffer);
            int length = (int)reader.UnconsumedBufferLength;
            for (int i = 0; i < length; ++i)
                buffer[offset + i] = reader.ReadByte();
            return length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var task  = Client.Socket.OutputStream.WriteAsync(buffer.AsBuffer(offset, count)).AsTask<uint, uint>();
            task.ConfigureAwait(false);
            task.Wait();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin) { return 0; }
        public override void SetLength(long value) { }
    }
}

#endif