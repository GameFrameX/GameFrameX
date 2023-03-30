using BestHTTP;
using System;
using System.IO;
using System.Threading;

namespace BestHTTP.Examples
{
    public sealed class UploadStream : Stream
    {
        #region Private Fields

        /// <summary>
        /// Buffer for reads
        /// </summary>
        MemoryStream ReadBuffer = new MemoryStream();

        /// <summary>
        /// Buffer for writes
        /// </summary>
        MemoryStream WriteBuffer = new MemoryStream();

        /// <summary>
        /// Indicates that we will not write more data to this stream
        /// </summary>
        bool noMoreData;

        /// <summary>
        /// For thread synchronization
        /// </summary>
        AutoResetEvent ARE = new AutoResetEvent(false);

        /// <summary>
        /// For thread synchronization
        /// </summary>
        object locker = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Name of this stream for easier debugging
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// true if we are read all data from the read buffer
        /// </summary>
        private bool IsReadBufferEmpty { get { lock (locker) return ReadBuffer.Position == ReadBuffer.Length; } }

        #endregion

        #region Constructors

        public UploadStream(string name)
            : this()
        {
            this.Name = name;
        }

        public UploadStream()
        {
            this.ReadBuffer = new MemoryStream();
            this.WriteBuffer = new MemoryStream();
            this.Name = string.Empty;
        }

        #endregion

        #region Stream Implementation

        public override int Read(byte[] buffer, int offset, int count)
        {
            // We will not push more data to the write buffer
            if (noMoreData)
            {
                // No data left in the read buffer
                if (ReadBuffer.Position == ReadBuffer.Length)
                {
                    // Is there any data in the write buffer? If so, switch the buffers
                    if (WriteBuffer.Length > 0)
                        SwitchBuffers();
                    else
                    {
                        HTTPManager.Logger.Information("UploadStream", string.Format("{0} - Read - End Of Stream", this.Name));
                        return -1;
                    }
                }
                else
                    return ReadBuffer.Read(buffer, offset, count);
            }

            // There are no more data in the read buffer? Wait for it.
            if (IsReadBufferEmpty)
            {
                ARE.WaitOne();

                lock (locker)
                    if (IsReadBufferEmpty && WriteBuffer.Length > 0)
                        SwitchBuffers();
            }

            int read = -1;

            lock (locker)
                read = ReadBuffer.Read(buffer, offset, count);

            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (noMoreData)
                throw new System.ArgumentException("noMoreData already set!");

            lock (locker)
            {
                WriteBuffer.Write(buffer, offset, count);

                SwitchBuffers();
            }

            ARE.Set();
        }

        public override void Flush()
        {
            Finish();
        }

        #endregion

        #region Dispose Implementation

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                HTTPManager.Logger.Information("UploadStream", string.Format("{0} - Dispose", this.Name));

                ReadBuffer.Dispose();
                ReadBuffer = null;

                WriteBuffer.Dispose();
                WriteBuffer = null;

#if NETFX_CORE
            ARE.Dispose();
#else
                ARE.Close();
#endif
                ARE = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Helper Functions

        public void Finish()
        {
            if (noMoreData)
                throw new System.ArgumentException("noMoreData already set!");

            HTTPManager.Logger.Information("UploadStream", string.Format("{0} - Finish", this.Name));

            noMoreData = true;

            ARE.Set();
        }

        private bool SwitchBuffers()
        {
            // Switch the buffers only when all data are consumed from our read buffer
            lock (locker)
            {
                if (ReadBuffer.Position == ReadBuffer.Length)
                {
                    // This buffer will be the read buffer, we need to seek back to the beginning
                    WriteBuffer.Seek(0, SeekOrigin.Begin);

                    // This will be the write buffer, set the length to zero
                    ReadBuffer.SetLength(0);

                    // switch the two buffers
                    MemoryStream tmp = WriteBuffer;
                    WriteBuffer = ReadBuffer;
                    ReadBuffer = tmp;

                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Not Implemented Functions and Properties

        public override bool CanRead { get { throw new NotImplementedException(); } }
        public override bool CanSeek { get { throw new NotImplementedException(); } }
        public override bool CanWrite { get { throw new NotImplementedException(); } }

        public override long Length { get { throw new NotImplementedException(); } }
        public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}