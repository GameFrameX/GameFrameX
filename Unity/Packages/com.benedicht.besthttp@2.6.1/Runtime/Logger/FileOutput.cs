using System;

using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.Logger
{
    public sealed class FileOutput : ILogOutput
    {
        private System.IO.Stream fileStream;

        public FileOutput(string fileName)
        {
            this.fileStream = HTTPManager.IOService.CreateFileStream(fileName, PlatformSupport.FileSystem.FileStreamModes.Create);
        }

        public void Write(Loglevels level, string logEntry)
        {
            if (this.fileStream != null && !string.IsNullOrEmpty(logEntry))
            {
                int count = System.Text.Encoding.UTF8.GetByteCount(logEntry);
                var buffer = BufferPool.Get(count, true);

                try
                {
                    System.Text.Encoding.UTF8.GetBytes(logEntry, 0, logEntry.Length, buffer, 0);

                    this.fileStream.Write(buffer, 0, count);
                    this.fileStream.WriteLine();
                }
                finally
                {
                    BufferPool.Release(buffer);
                }

                this.fileStream.Flush();
            }
        }

        public void Dispose()
        {
            if (this.fileStream != null)
            {
                this.fileStream.Close();
                this.fileStream = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
