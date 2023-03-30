#if NETFX_CORE && !UNITY_EDITOR && !ENABLE_IL2CPP

using System;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace BestHTTP.PlatformSupport.TcpClient.WinRT
{
    public sealed class TcpClient : IDisposable
    {
#region Public Properties

        public bool Connected { get; private set; }
        public TimeSpan ConnectTimeout { get; set; }
#endregion

#region Private Properties

        internal StreamSocket Socket { get; set; }
        private System.IO.Stream Stream { get; set; }

#endregion

        public TcpClient()
        {
            ConnectTimeout = TimeSpan.FromSeconds(2);
        }

        public void Connect(string hostName, int port)
        {
            //How to secure socket connections with TLS/SSL:
            //http://msdn.microsoft.com/en-us/library/windows/apps/jj150597.aspx

            //Networking in Windows 8 Apps - Using StreamSocket for TCP Communication
            //http://blogs.msdn.com/b/metulev/archive/2012/10/22/networking-in-windows-8-apps-using-streamsocket-for-tcp-communication.aspx

            Socket = new StreamSocket();
            Socket.Control.KeepAlive = true;
            Socket.Control.NoDelay = true;

            var host = new HostName(hostName);

            System.Threading.CancellationTokenSource tokenSource = new System.Threading.CancellationTokenSource();

            // https://msdn.microsoft.com/en-us/library/windows/apps/xaml/jj710176.aspx#content
            try
            {
                if (ConnectTimeout > TimeSpan.Zero)
                    tokenSource.CancelAfter(ConnectTimeout);

                var task = Socket.ConnectAsync(host, port.ToString(), SocketProtectionLevel.PlainSocket).AsTask(tokenSource.Token);
                task.ConfigureAwait(false);
                task.Wait();
                Connected = task.IsCompleted;
            }
            catch(AggregateException ex)
            {
                //https://msdn.microsoft.com/en-us/library/dd537614(v=vs.110).aspx?f=255&MSPPError=-2147217396

                Connected = false;
                if (ex.InnerException != null)
                    //throw ex.InnerException;
                {
                    if ( ex.Message.Contains("No such host is known") || ex.Message.Contains("unreachable host") )
                        throw new Exception("Socket Exception: " + ex.Message);
                    else
                        throw ex.InnerException;
                }
                else
                    throw ex;
            }
            finally {
                // https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource
                tokenSource.Dispose();
            }

            if (!Connected)
                throw new TimeoutException("Connection timed out!");
        }

        public bool IsConnected()
        {
            return true;
        }

        public System.IO.Stream GetStream()
        {
            if (Stream == null)
                Stream = new DataReaderWriterStream(this);
            return Stream;
        }

        public void Close()
        {
            Dispose();
        }

#region IDisposeble

        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                if (disposing)
                {
                    if (Stream != null)
                        Stream.Dispose();
                    Stream = null;
                    Connected = false;
                    this.Socket.Dispose();
                }
            }
        }

        ~TcpClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#endregion
    }
}

#endif