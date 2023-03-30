using System;
using System.Collections.Generic;
using System.IO;
using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP
{
    /// <summary>
    /// Stream based implementation of the multipart/form-data Content-Type. Using this class reading a whole file into memory can be avoided.
    /// This implementation expects that all streams has a final, accessible Length.
    /// </summary>
    public sealed class MultipartFormDataStream : System.IO.Stream
    {
        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return false; } }

        public override long Length
        {
            get
            {
                // multipart/form-data requires a leading boundary that we can add when all streams are added.
                // This final preparation could be user initiated, but we can do it automatically too when the HTTPRequest
                // first access the Length property.
                if (!this.prepared)
                {
                    this.prepared = true;
                    this.Prepare();
                }

                return this._length;
            }
        }
        private long _length;

        public override long Position { get; set; }
        /// <summary>
        /// A random boundary generated in the constructor.
        /// </summary>
        private string boundary;

        private Queue<StreamList> fields = new Queue<StreamList>(1);
        private StreamList currentField;
        private bool prepared;

        public MultipartFormDataStream(HTTPRequest request)
        {
            this.boundary = "BestHTTP_MultipartFormDataStream_" + this.GetHashCode().ToString("X2");

            request.SetHeader("Content-Type", "multipart/form-data; boundary=" + boundary);
            request.UploadStream = this;
            request.UseUploadStreamLength = true;
        }

        public void AddField(string fieldName, string value)
        {
            AddField(fieldName, value, System.Text.Encoding.UTF8);
        }

        public void AddField(string fieldName, string value, System.Text.Encoding encoding)
        {
            var enc = encoding ?? System.Text.Encoding.UTF8;
            var byteCount = enc.GetByteCount(value);
            var buffer = BufferPool.Get(byteCount, true);
            var stream = new BufferPoolMemoryStream();

            enc.GetBytes(value, 0, value.Length, buffer, 0);

            stream.Write(buffer, 0, byteCount);

            stream.Position = 0;

            string mime = encoding != null ? "text/plain; charset=" + encoding.WebName : null;
            AddStreamField(stream, fieldName, null, mime);
        }

        public void AddStreamField(System.IO.Stream stream, string fieldName)
        {
            AddStreamField(stream, fieldName, null, null);
        }

        public void AddStreamField(System.IO.Stream stream, string fieldName, string fileName)
        {
            AddStreamField(stream, fieldName, fileName, null);
        }

        public void AddStreamField(System.IO.Stream stream, string fieldName, string fileName, string mimeType)
        {
            var header = new BufferPoolMemoryStream();
            header.WriteLine("--" + this.boundary);
            header.WriteLine("Content-Disposition: form-data; name=\"" + fieldName + "\"" + (!string.IsNullOrEmpty(fileName) ? "; filename=\"" + fileName + "\"" : string.Empty));
            // Set up Content-Type head for the form.
            if (!string.IsNullOrEmpty(mimeType))
                header.WriteLine("Content-Type: " + mimeType);
            //header.WriteLine("Content-Length: " + stream.Length.ToString());
            header.WriteLine();
            header.Position = 0;

            var footer = new BufferPoolMemoryStream();
            footer.Write(HTTPRequest.EOL, 0, HTTPRequest.EOL.Length);
            footer.Position = 0;

            // all wrapped streams going to be disposed by the StreamList wrapper.
            var wrapper = new StreamList(header, stream, footer);

            try
            {
                if (this._length >= 0)
                    this._length += wrapper.Length;
            }
            catch
            {
                this._length = -1;
            }

            this.fields.Enqueue(wrapper);
        }

        /// <summary>
        /// Adds the final boundary.
        /// </summary>
        private void Prepare()
        {
            var boundaryStream = new BufferPoolMemoryStream();
            boundaryStream.WriteLine("--" + this.boundary + "--");
            boundaryStream.Position = 0;

            this.fields.Enqueue(new StreamList(boundaryStream));

            if (this._length >= 0)
                this._length += boundaryStream.Length;
        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            if (this.currentField == null && this.fields.Count == 0)
                return -1;

            if (this.currentField == null && this.fields.Count > 0)
                this.currentField = this.fields.Dequeue();

            int readCount = 0;

            do
            {
                // read from the current stream
                int count = this.currentField.Read(buffer, offset + readCount, length - readCount);

                if (count > 0)
                    readCount += count;
                else
                {
                    // if the current field's stream is empty, go for the next one.

                    // dispose the current one first
                    try
                    {
                        this.currentField.Dispose();
                    }
                    catch
                    { }

                    // no more fields/streams? exit
                    if (this.fields.Count == 0)
                        break;

                    // grab the next one
                    this.currentField = this.fields.Dequeue();
                }

                // exit when we reach the length goal, or there's no more streams to read from
            } while (readCount < length && this.fields.Count > 0);

            return readCount;
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
            throw new NotImplementedException();
        }

        public override void Flush() { }
    }
}
