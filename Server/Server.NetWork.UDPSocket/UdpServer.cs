using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Geek.Server.Core.Net.Kcp;
using Server.Extension;
using Server.NetWork.Messages;
using Server.Setting;
using Server.Utility;


namespace Server.NetWork.UDPSocket
{
    /// <summary>
    /// Udp server
    /// </summary>
    public sealed class UdpServer
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public static IMessageHelper? MessageHelper { get; private set; }

        public delegate void ReceiveAction(EndPoint point, TempNetPackage package);

        Func<int, EndPoint> getEndPointById;

        /// <summary>
        /// 服务ID
        /// </summary>
        readonly int innerId = 0;

        /// <summary>
        /// 是否是内部服务
        /// </summary>
        bool IsInnerServer => innerId != 0;

        private Socket? socket;

        public UdpServer(int port, int innerId = 0, ReceiveAction onMessage = null)
        {
            this.innerId = innerId;
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            socket.DualMode = true;
            socket.EnableBroadcast = true;
            this.onReceiveAction = onMessage;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                socket.SendBufferSize = 1024 * 1024 * 40;
                socket.ReceiveBufferSize = 1024 * 1024 * 40;
            }

            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    const uint iocIn = 0x80000000;
                    const uint iocVendor = 0x18000000;
                    const int sioUdpConnectReset = unchecked((int)(iocIn | iocVendor | 12));
                    socket.IOControl(sioUdpConnectReset, new[] { Convert.ToByte(false) }, null);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"udp bind error: {port}", e);
            }
        }

        readonly ReceiveAction onReceiveAction;

        void StartFunction(object? o)
        {
            var cache = ArrayPool<byte>.Shared.Rent(4096);
            EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (socket != null)
            {
                try
                {
                    int len = socket.ReceiveFrom(cache, ref ipEndPoint);
                    if (IsInnerServer)
                    {
                        ipEndPoint = getEndPointById?.Invoke(cache.ReadInt(0)) ?? ipEndPoint;
                    }

                    //LOGGER.Info($"收到udp数据...{ipEndPoint} {len}");
                    if (onReceiveAction != null)
                    {
                        var offset = IsInnerServer ? 4 : 0;
                        var data = cache.AsSpan(offset, len - offset);

                        var package = new TempNetPackage(data);
                        if (package.IsOk)
                        {
                            // if (package.flag != NetPackageFlag.HEART)
                            {
                                Log.Info($"收到包...{package.ToString()}");
                            }
                            onReceiveAction(ipEndPoint, package);
                        }
                        else
                        {
                            Log.Error($"错误的udp package...{ipEndPoint}");
                        }
                    }
                }
                catch (Exception e)
                {
                    if (GlobalSettings.IsDebug)
                    {
                        Log.Warn(e);
                    }
                }
            }

            Log.Warn($"退出udp接收线程...{o}");
        }

        public Task Start(IMessageHelper messageHelper, Func<int, EndPoint> getEndPointById = null, int parallel = 1)
        {
            Guard.NotNull(messageHelper, nameof(messageHelper));
            MessageHelper = messageHelper;
            this.getEndPointById = getEndPointById;
            parallel = Math.Max(parallel, 1);

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < parallel; i++)
            {
                tasks.Add(Task.Factory.StartNew(StartFunction, TaskCreationOptions.LongRunning));
            }

            return Task.WhenAll(tasks);
        }

        public void SendTo(TempNetPackage package, EndPoint point)
        {
            var len = package.Length + (innerId == 0 ? 0 : 4);
            Span<byte> target = stackalloc byte[len];
            int offset = 0;
            if (IsInnerServer)
            {
                target.Write(innerId, ref offset);
            }

            Write(target, package, ref offset);
            try
            {
                socket?.SendTo(target, point);

                if (package.Flag != NetPackageFlag.HEART)
                {
                    Log.Info($"发送包...{package.ToString()} {point}");
                }
            }
            catch
            {
            }
        }

        public void SendTo(ReadOnlySpan<byte> span, EndPoint point)
        {
            try
            {
                if (IsInnerServer)
                {
                    int offset = 0;
                    Span<byte> target = stackalloc byte[span.Length + 4];
                    target.Write(innerId, ref offset);
                    span.CopyTo(target[offset..]);
                    socket?.SendTo(target, point);
                }
                else
                    socket?.SendTo(span, point);
            }
            catch
            {
            }
        }

        public void Stop()
        {
            socket?.Close();
            socket = null;
        }

        public static void Write(Span<byte> buffer, TempNetPackage package, ref int offset)
        {
            if (buffer.Length - offset < package.Length)
            {
                throw new ArgumentException($"Write out of index {buffer.Length}, {package.Length}");
            }

            buffer[offset] = package.Flag;
            offset++;
            buffer.Write(package.NetId, ref offset);
            buffer.Write(package.InnerServerId, ref offset);
            if (!package.Body.IsEmpty)
            {
                package.Body.CopyTo(buffer[offset..]);
            }
        }
    }
}