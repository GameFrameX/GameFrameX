/*using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Geek.Server.Core.Net.Kcp;
using NetWork;

using Server.NetWork.Messages;
using Server.NetWork.UDPSocket;

namespace Server.NetWork.KCPSocket
{
    public sealed class KcpServer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ConcurrentDictionary<long, KcpChannel> channels = new();
        private readonly SemaphoreSlim newChannArrived = new(initialCount: 0, maxCount: int.MaxValue);
        private readonly IMessageHelper messageHelper;
        private readonly Func<KcpChannel, IMessage, Task> onMessageAct;
        private readonly Action<KcpChannel> onChannelRemove;
        private readonly Func<int, EndPoint> getPointById;
        private readonly UdpServer udpServer;
        readonly CancellationTokenSource closeSrc = new CancellationTokenSource();

        public KcpServer(int port, IMessageHelper messageHelper, Func<KcpChannel, IMessage, Task> onMessageAct, Action<KcpChannel> onChannelRemove, Func<int, EndPoint> getPointById = null)
        {
            Log.Info($"开始kcp server...{port}");
            this.messageHelper = messageHelper;
            this.onMessageAct = onMessageAct;
            this.onChannelRemove = onChannelRemove;
            this.getPointById = getPointById;
            udpServer = new UdpServer(port, 1000, OnRecv);
        }

        public void Start()
        {
            _ = udpServer.Start(messageHelper, getPointById);
            _ = UpdateChannel();
        }

        public void Stop()
        {
            closeSrc?.Cancel();
            udpServer.Stop();
            channels.Clear();
            //foreach (long channelId in channels.Keys.ToArray())
            //{
            //    channels.TryRemove(channelId, out var channel);
            //    channel?.Close();
            //}
        }

        async Task UpdateChannel()
        {
            List<KcpChannel> channelList = new();
            var paraOpt = new ParallelOptions { MaxDegreeOfParallelism = 3 };
            var token = closeSrc.Token;
            while (true)
            {
                try
                {
                    if (channels.IsEmpty)
                    {
                        await newChannArrived.WaitAsync(token);
                    }

                    if (token.IsCancellationRequested)
                        return;

                    var time = DateTime.UtcNow;
                    channelList.Clear();
                    foreach (var kv in channels)
                    {
                        channelList.Add(kv.Value);
                    }

                    Parallel.ForEach(channelList, paraOpt, (channel) =>
                    {
                        if (token.IsCancellationRequested)
                            return;
                        try
                        {
                            channel.Update(time);
                        }
                        catch (Exception e)
                        {
                            Log.Error("kcp channel update error:" + e.Message);
                        }

                        if (channel.IsClose())
                        {
                            Log.Info($"移除channel:{channel.RemoteAddress}");
                            channels.Remove(channel.NetId, out _);
                            onChannelRemove?.Invoke(channel);
                        }
                    });
                    await Task.Delay(10, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        void OnRecv(EndPoint ipEndPoint, TempNetPackage package)
        {
            //LOGGER.Info($"kcp server 收到包:{package.ToString()}");
            long netId = package.NetId;
            int innerServerNetId = package.InnerServerId;
            var curServerId = 1000;

            //检查内网服务器id
            if (innerServerNetId == 0 || innerServerNetId != 1000)
            {
                Log.Warn($"kcp消息错误,不正确的内网服id:{innerServerNetId}");
                return;
            }

            KcpChannel channel = null;
            channels.TryGetValue(netId, out channel);
            if (channel == null || channel.IsClose())
            {
                channel = null;
            }
            else
            {
                channel.UpdateReceiveMessageTime();
            }

            try
            {
                switch (package.Flag)
                {
                    case NetPackageFlag.SYN:
                        if (channel == null || channel.IsClose())
                        {
                            channel = new KcpChannel(true, netId, curServerId, ipEndPoint, (chann, data) =>
                            {
                                var tmpPackage = new TempNetPackage(NetPackageFlag.MSG, chann.NetId, chann.TargetServerId, data);
                                //LOGGER.Info($"kcp发送udp数据到gate:{(chann as KcpChannel).routerEndPoint?.ToString()}");
                                udpServer.SendTo(tmpPackage, (chann as KcpChannel).routerEndPoint);
                            }, onMessageAct);
                            channels[channel.NetId] = channel;
                            newChannArrived.Release();
                            channel.RemoteAddress = Encoding.UTF8.GetString(package.Body);
                        }

                        //更新最新路由地址
                        var endPoint = ipEndPoint as IPEndPoint;
                        channel.routerEndPoint = new IPEndPoint(endPoint.Address, endPoint.Port);
                        channel.UpdateReceiveMessageTime(TimeSpan.FromSeconds(5).Ticks);

                        udpServer.SendTo(new TempNetPackage(NetPackageFlag.ACK, netId, curServerId), ipEndPoint);
                        Log.Info($"kcp server 收到请求 建立连接:{package.ToString()}");
                        break;

                    case NetPackageFlag.SYN_OLD_NET_ID:
                        if (channel == null || channel.IsClose())
                        {
                            udpServer.SendTo(new TempNetPackage(NetPackageFlag.CLOSE, netId, curServerId), ipEndPoint);
                        }
                        else
                        {
                            udpServer.SendTo(new TempNetPackage(NetPackageFlag.ACK, netId, curServerId), ipEndPoint);
                            channel.UpdateReceiveMessageTime(TimeSpan.FromSeconds(5).Ticks);
                        }

                        break;

                    case NetPackageFlag.MSG:
                        if (channel == null)
                        {
                            Log.Info($"kcpservice recv msg, channel 不存在: {netId}");
                            udpServer.SendTo(new TempNetPackage(NetPackageFlag.CLOSE, netId, curServerId), ipEndPoint);
                        }
                        else
                        {
                            channel.HandleRecv(package.Body);
                        }

                        break;

                    case NetPackageFlag.HEART:
                        if (channel == null || channel.IsClose())
                        {
                            Log.Info($"kcpservice recv gate_heart, channel 不存在: {netId}");
                            udpServer.SendTo(new TempNetPackage(NetPackageFlag.CLOSE, netId, curServerId), ipEndPoint);
                        }
                        else
                        {
                            channel.UpdateReceiveMessageTime();
                        }

                        break;

                    case NetPackageFlag.NO_GATE_CONNECT:
                        channel?.UpdateReceiveMessageTime(TimeSpan.FromSeconds(-5).Ticks);
                        break;

                    case NetPackageFlag.CLOSE:
                        channel?.Close();
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error($"kcpservice error:{e}");
            }
        }
    }
}*/