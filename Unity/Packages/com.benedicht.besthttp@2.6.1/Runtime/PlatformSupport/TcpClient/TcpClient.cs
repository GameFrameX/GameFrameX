#if !NETFX_CORE || UNITY_EDITOR

// TcpClient.cs
//
// Author:
// 	Phillip Pearson (pp@myelin.co.nz)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Sridhar Kulkarni (sridharkulkarni@gmail.com)
//	Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2001, Phillip Pearson http://www.myelin.co.nz
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
// Copyright 2011 Xamarin Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace BestHTTP.PlatformSupport.TcpClient.General
{
    // This is a little modified TcpClient class from the Mono src tree.
    public class TcpClient : IDisposable
    {
        enum Properties : uint
        {
            LingerState = 1,
            NoDelay = 2,
            ReceiveBufferSize = 4,
            ReceiveTimeout = 8,
            SendBufferSize = 16,
            SendTimeout = 32
        }

        // private data
        NetworkStream stream;
        bool active;
        Socket client;
        bool disposed;
        Properties values;
        int recv_timeout, send_timeout;
        int recv_buffer_size, send_buffer_size;
        LingerOption linger_state;
        bool no_delay;

        private void Init(AddressFamily family)
        {
            active = false;

            if (client != null)
            {
                client.Close();
                client = null;
            }

            client = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
        }

        public TcpClient()
        {
            Init(AddressFamily.InterNetwork);
            //client.Bind(new IPEndPoint(IPAddress.Any, 0));

            ConnectTimeout = TimeSpan.FromSeconds(2);
        }

        public TcpClient(AddressFamily family)
        {
            if (family != AddressFamily.InterNetwork &&
                family != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Family must be InterNetwork or InterNetworkV6", "family");
            }

            Init(family);
            /*IPAddress any = IPAddress.Any;
            if (family == AddressFamily.InterNetworkV6)
                any = IPAddress.IPv6Any;
            client.Bind(new IPEndPoint(any, 0));*/

            ConnectTimeout = TimeSpan.FromSeconds(2);
        }

        public TcpClient(IPEndPoint localEP)
        {
            Init(localEP.AddressFamily);
            //client.Bind(localEP);

            ConnectTimeout = TimeSpan.FromSeconds(2);
        }

        public TcpClient(string hostname, int port)
        {
            ConnectTimeout = TimeSpan.FromSeconds(2);

            Connect(hostname, port);
        }

        protected bool Active
        {
            get { return active; }
            set { active = value; }
        }

        public Socket Client
        {
            get { return client; }
            set
            {
                client = value;
                stream = null;
            }
        }

        public int Available
        {
            get { return client.Available; }
        }

        public bool Connected
        {
            get { return client.Connected; }
        }


        public bool IsConnected()
        {
            try
            {
                return !(Client.Poll(1, SelectMode.SelectRead) && Client.Available == 0);
            }
            catch (Exception) { return false; }
        }

        public bool ExclusiveAddressUse
        {
            get
            {
                return (client.ExclusiveAddressUse);
            }
            set
            {
                client.ExclusiveAddressUse = value;
            }
        }

        internal void SetTcpClient(Socket s)
        {
            Client = s;
        }

        public LingerOption LingerState
        {
            get
            {
                if ((values & Properties.LingerState) != 0)
                    return linger_state;

                return (LingerOption)client.GetSocketOption(SocketOptionLevel.Socket,
                                    SocketOptionName.Linger);
            }
            set
            {
                if (!client.Connected)
                {
                    linger_state = value;
                    values |= Properties.LingerState;
                    return;
                }
                client.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.Linger, value);
            }
        }

        public bool NoDelay
        {
            get
            {
                if ((values & Properties.NoDelay) != 0)
                    return no_delay;

                return (bool)client.GetSocketOption(
                    SocketOptionLevel.Tcp,
                    SocketOptionName.NoDelay);
            }
            set
            {
                if (!client.Connected)
                {
                    no_delay = value;
                    values |= Properties.NoDelay;
                    return;
                }
                client.SetSocketOption(
                    SocketOptionLevel.Tcp,
                    SocketOptionName.NoDelay, value ? 1 : 0);
            }
        }

        public int ReceiveBufferSize
        {
            get
            {
                if ((values & Properties.ReceiveBufferSize) != 0)
                    return recv_buffer_size;

                return (int)client.GetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReceiveBuffer);
            }
            set
            {
                if (!client.Connected)
                {
                    recv_buffer_size = value;
                    values |= Properties.ReceiveBufferSize;
                    return;
                }
                client.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReceiveBuffer, value);
            }
        }

        public int ReceiveTimeout
        {
            get
            {
                if ((values & Properties.ReceiveTimeout) != 0)
                    return recv_timeout;

                return (int)client.GetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReceiveTimeout);
            }
            set
            {
                if (!client.Connected)
                {
                    recv_timeout = value;
                    values |= Properties.ReceiveTimeout;
                    return;
                }
                client.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.ReceiveTimeout, value);
            }
        }

        public int SendBufferSize
        {
            get
            {
                if ((values & Properties.SendBufferSize) != 0)
                    return send_buffer_size;

                return (int)client.GetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.SendBuffer);
            }
            set
            {
                if (!client.Connected)
                {
                    send_buffer_size = value;
                    values |= Properties.SendBufferSize;
                    return;
                }
                client.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.SendBuffer, value);
            }
        }

        public int SendTimeout
        {
            get
            {
                if ((values & Properties.SendTimeout) != 0)
                    return send_timeout;

                return (int)client.GetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.SendTimeout);
            }
            set
            {
                if (!client.Connected)
                {
                    send_timeout = value;
                    values |= Properties.SendTimeout;
                    return;
                }
                client.SetSocketOption(
                    SocketOptionLevel.Socket,
                    SocketOptionName.SendTimeout, value);
            }
        }

        public TimeSpan ConnectTimeout { get; set; }

        // methods

        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        public void Connect(IPEndPoint remoteEP)
        {
            try
            {
                if (ConnectTimeout > TimeSpan.Zero)
                {
                    // Third version, works in WebPlayer
                    System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false);
                    IAsyncResult result = client.BeginConnect(remoteEP, (res) => mre.Set(), null);
                    active = mre.WaitOne(ConnectTimeout);
                    if (active)
                        client.EndConnect(result);
                    else
                    {
                        try
                        {
                            client.Disconnect(true);
                        }
                        catch
                        { }

                        throw new TimeoutException("Connection timed out!");
                    }

                    // Second version with timeout, in WebPlayer can't connect:
                    // Attempt to access a private/protected method failed. at System.Security.SecurityManager.ThrowException (System.Exception ex) [0x00000] in <filename unknown>:0
                    /*IAsyncResult result = client.BeginConnect(remoteEP, null, null);
                    Active = result.AsyncWaitHandle.WaitOne(ConnectTimeout, true);
                    if (active)
                    {
                        client.EndConnect(result);
                    }
                    else
                    {
                        client.Close();
                        //throw new SocketException(10060);
                        throw new TimeoutException("Connection timed out!");
                    }*/
                }
                else
                {
                    // First(old) version, no timeout
                    client.Connect(remoteEP);
                    active = true;
                }
            }
            finally
            {
                CheckDisposed();
            }
        }

        public void Connect(IPAddress address, int port)
        {
            Connect(new IPEndPoint(address, port));
        }

        void SetOptions()
        {
            Properties props = values;
            values = 0;

            if ((props & Properties.LingerState) != 0)
                LingerState = linger_state;
            if ((props & Properties.NoDelay) != 0)
                NoDelay = no_delay;
            if ((props & Properties.ReceiveBufferSize) != 0)
                ReceiveBufferSize = recv_buffer_size;
            if ((props & Properties.ReceiveTimeout) != 0)
                ReceiveTimeout = recv_timeout;
            if ((props & Properties.SendBufferSize) != 0)
                SendBufferSize = send_buffer_size;
            if ((props & Properties.SendTimeout) != 0)
                SendTimeout = send_timeout;
        }

        public void Connect(string hostname, int port)
        {
            if (ConnectTimeout > TimeSpan.Zero)
            {
                // https://forum.unity3d.com/threads/best-http-released.200006/page-37#post-3150972
                System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false);
                IAsyncResult result = Dns.BeginGetHostAddresses(hostname, (res) => mre.Set(), null);
                bool success = mre.WaitOne(ConnectTimeout);
                if (success)
                {
                    IPAddress[] addresses = Dns.EndGetHostAddresses(result);
                    Connect(addresses, port, null);
                }
                else
                {
                    throw new TimeoutException("DNS resolve timed out!");
                }
            }
            else
            {
                IPAddress[] addresses = Dns.GetHostAddresses(hostname);
                Connect(addresses, port, null);
            }
        }

        public void Connect(IPAddress[] ipAddresses, int port, HTTPRequest request)
        {
            CheckDisposed();

            if (ipAddresses == null)
            {
                throw new ArgumentNullException("ipAddresses");
            }

            for (int i = 0; i < ipAddresses.Length; i++)
            {
                try
                {
                    IPAddress address = ipAddresses[i];

                    if (address.Equals(IPAddress.Any) ||
                        address.Equals(IPAddress.IPv6Any))
                    {
                        throw new SocketException((int)SocketError.AddressNotAvailable);
                    }

                    Init(address.AddressFamily);

                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        //client.Bind(new IPEndPoint(IPAddress.Any, 0));
                    }
                    else if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        //client.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
                    }
                    else
                    {
                        throw new NotSupportedException("This method is only valid for sockets in the InterNetwork and InterNetworkV6 families");
                    }

                    if (request != null && request.IsCancellationRequested)
                        throw new Exception("IsCancellationRequested");

                    Connect(new IPEndPoint(address, port));

                    if (values != 0)
                    {
                        SetOptions();
                    }

                    try
                    {
                        // Enable Keep-Alive packets
                        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                        /*
                            TCP_KEEPIDLE		4	 // Start keeplives after this period
                            TCP_KEEPINTVL		5	 // Interval between keepalives
                            TCP_KEEPCNT		  6    // Number of keepalives before death
                        */

                        //client.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)4, 30);
                        //client.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)5, 10);
                    }
                    catch { }


#if UNITY_WINDOWS || UNITY_EDITOR
                    // Set the keep-alive time and interval on windows

                    // https://msdn.microsoft.com/en-us/library/windows/desktop/dd877220%28v=vs.85%29.aspx
                    // https://msdn.microsoft.com/en-us/library/windows/desktop/ee470551%28v=vs.85%29.aspx
                    try
                    {
                        //SetKeepAlive(true, 30000, 1000);
                    }
                    catch{ }
#endif

                    HTTPManager.Logger.Information("TcpClient", string.Format("Connected to {0}:{1}", address.ToString(), port.ToString()), request.Context);

                    break;
                }
                catch (Exception e)
                {
                    /* Reinitialise the socket so
                     * other properties still work
                     * (see no-arg constructor)
                     */
                    Init(AddressFamily.InterNetwork);

                    /* This is the last known
                     * address, so re-throw the
                     * exception
                     */
                    if (i == ipAddresses.Length - 1)
                    {
                        throw e;
                    }
                }
            }
        }

        public void EndConnect(IAsyncResult asyncResult)
        {
            client.EndConnect(asyncResult);
        }

        public IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            return client.BeginConnect(address, port, requestCallback, state);
        }

        public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            return client.BeginConnect(addresses, port, requestCallback, state);
        }

        public IAsyncResult BeginConnect(string host, int port, AsyncCallback requestCallback, object state)
        {
            return client.BeginConnect(host, port, requestCallback, state);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;

            if (disposing)
            {
                // release managed resources
                NetworkStream s = stream;
                stream = null;
                if (s != null)
                {
                    // This closes the socket as well, as the NetworkStream
                    // owns the socket.
                    s.Close();
                    active = false;
                    s = null;
                }
                else if (client != null)
                {
                    client.Close();
                    client = null;
                }
            }
        }

        ~TcpClient()
        {
            Dispose(false);
        }

        public Stream GetStream()
        {
            try
            {
                if (stream == null)
                    stream = new NetworkStream(client, true);
                return stream;
            }
            finally { CheckDisposed(); }
        }

        private void CheckDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

#if UNITY_WINDOWS || UNITY_EDITOR
        public void SetKeepAlive(bool on, uint keepAliveTime, uint keepAliveInterval)
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(new uint());

            var inOptionValues = new byte[size * 3];

            BitConverter.GetBytes((uint)(on ? 1 : 0)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(inOptionValues, size);
            BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(inOptionValues, size * 2);

            //client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
            int dwBytesRet = 0;
            WSAIoctl(client.Handle, /*SIO_KEEPALIVE_VALS*/ System.Net.Sockets.IOControlCode.KeepAliveValues, inOptionValues, inOptionValues.Length, /*NULL*/IntPtr.Zero, 0, ref dwBytesRet, /*NULL*/IntPtr.Zero, /*NULL*/IntPtr.Zero);
        }

        [System.Runtime.InteropServices.DllImport("Ws2_32.dll")]
        public static extern int WSAIoctl(
            /* Socket, Mode */               IntPtr s, System.Net.Sockets.IOControlCode dwIoControlCode,
            /* Optional Or IntPtr.Zero, 0 */ byte[] lpvInBuffer, int cbInBuffer,
            /* Optional Or IntPtr.Zero, 0 */ IntPtr lpvOutBuffer, int cbOutBuffer,
            /* reference to receive Size */  ref int lpcbBytesReturned,
            /* IntPtr.Zero, IntPtr.Zero */   IntPtr lpOverlapped, IntPtr lpCompletionRoutine);
#endif
    }
}

#endif
