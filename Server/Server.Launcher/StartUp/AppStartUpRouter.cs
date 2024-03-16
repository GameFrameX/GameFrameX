using Newtonsoft.Json;
using Server.Launcher.PipelineFilter;
using Server.NetWork.TCPSocket;
using SuperSocket.Channel;
using ErrorEventArgs = SuperSocket.ClientEngine.ErrorEventArgs;
using Timer = System.Timers.Timer;

/// <summary>
/// 路由服务器.最后启动。
/// </summary>
[StartUpTag(ServerType.Router, Int32.MaxValue)]
internal sealed class AppStartUpRouter : AppStartUpBase
{
    /// <summary>
    /// 链接到网关的客户端
    /// </summary>
    private AsyncTcpSession client;

    /// <summary>
    /// 服务器。对外提供服务
    /// </summary>
    IServer server;

    /// <summary>
    /// 重连
    /// </summary>
    Timer reconnectionTimer;

    /// <summary>
    /// 心跳
    /// </summary>
    private Timer heartBeatTimer;

    public override async Task EnterAsync()
    {
        try
        {
            LogHelper.Info($"开始启动服务器{ServerType}");
            heartBeatTimer = new System.Timers.Timer(5000);
            heartBeatTimer.Elapsed += HeartBeatTimerOnElapsed;

            reconnectionTimer = new System.Timers.Timer(5000);
            reconnectionTimer.Elapsed += ReconnectionTimerOnElapsed;

            await StartServer();

            LogHelper.Info($"启动服务器 {ServerType} 端口: {Setting.TcpPort} 结束!");
            StartClient();
            TimeSpan delay = TimeSpan.FromSeconds(5);
            await Task.Delay(delay);
            if (client.IsConnected)
            {
                heartBeatTimer.Start();
            }

            await AppExitToken;
            Console.Write("全部断开...");
            heartBeatTimer.Close();
            reconnectionTimer.Close();
            await server.StopAsync();
            client.Close();
            LogHelper.Info("Done!");
        }
        catch (Exception e)
        {
        }
    }

    /// <summary>
    /// 重连定时器
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ReconnectionTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        // 重连到网关服务器
        client.Connect(new IPEndPoint(IPAddress.Parse(Setting.CenterUrl), Setting.GrpcPort));
    }

    /// <summary>
    /// 心跳定时器
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HeartBeatTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
    }

    private void StartClient()
    {
        client = new AsyncTcpSession();
        client.Connected += ClientOnConnected;
        client.Closed += ClientOnClosed;
        client.Error += ClientOnError;
        client.DataReceived += ClientOnDataReceived;
        LogHelper.Info("开始链接到网关服务器 ...");
        client.Connect(new IPEndPoint(IPAddress.Parse(Setting.CenterUrl), Setting.GrpcPort));
    }

    private void ClientOnError(object sender, ErrorEventArgs e)
    {
        LogHelper.Error("和网关服务器链接链接失败!.下一次重连时间:" + DateTime.Now.AddMilliseconds(reconnectionTimer.Interval));
        // 和网关服务器链接失败，开启重连
        reconnectionTimer.Start();
    }

    private void ClientOnDataReceived(object sender, DataEventArgs e)
    {
        LogHelper.Info("收到网关服务器返回的消息!" + e);
    }

    private void ClientOnClosed(object sender, EventArgs e)
    {
        LogHelper.Info("和网关服务器链接链接断开!");
        // 和网关服务器链接断开，开启重连
        reconnectionTimer.Start();
    }

    private void ClientOnConnected(object sender, EventArgs e)
    {
        LogHelper.Info("和网关服务器链接链接成功!");
        // 和网关服务器链接成功，关闭重连
        reconnectionTimer.Stop();
        // 开启和网关服务器的心跳
        heartBeatTimer.Start();
    }


    private async Task StartServer()
    {
        server = SuperSocketHostBuilder.Create<IMessage, MessageObjectPipelineFilter>()
            .ConfigureSuperSocket(ConfigureSuperSocket)
            .UseSessionFactory<GameSessionFactory>()
            .UseClearIdleSession()
            .UsePackageDecoder<MessageActorDiscoveryDecoderHandler>()
            .UsePackageEncoder<MessageActorDiscoveryEncoderHandler>()
            .UseSessionHandler(OnConnected, OnDisconnected)
            .UsePackageHandler(PackageHandler)
            .UseInProcSessionContainer()
            .BuildAsServer();

        await server.StartAsync();
    }

    readonly MessageActorDiscoveryEncoderHandler messageEncoderHandler = new MessageActorDiscoveryEncoderHandler();

    private async ValueTask PackageHandler(IAppSession session, IMessage messageObject)
    {
        if (messageObject is MessageObject msg)
        {
            var messageId = msg.MessageId;
            if (Setting.IsDebug && Setting.IsDebugReceive)
            {
                LogHelper.Debug($"---收到消息ID:[{messageId}] ==>消息类型:{msg.GetType()} 消息内容:{messageObject}");
            }
        }

        // 发送
        var response = new RespActorHeartBeat()
        {
            Timestamp = TimeHelper.UnixTimeSeconds()
        };
        await session.SendAsync(messageEncoderHandler, response);
    }


    private ValueTask OnConnected(IAppSession appSession)
    {
        LogHelper.Info("有外部客户端网络连接成功！。链接信息：SessionID:" + appSession.SessionID + " RemoteEndPoint:" + appSession.RemoteEndPoint);
        // NamingServiceManager.Instance.Add();
        return ValueTask.CompletedTask;
    }

    private ValueTask OnDisconnected(object sender, CloseEventArgs args)
    {
        LogHelper.Info("有外部客户端网络断开连接成功！。断开信息：" + args.Reason);
        return ValueTask.CompletedTask;
    }

    private void ConfigureSuperSocket(ServerOptions options)
    {
        options.AddListener(new ListenOptions { Ip = IPAddress.Any.ToString(), Port = Setting.TcpPort });
    }

    public override async Task Stop(string message = "")
    {
        heartBeatTimer.Close();
        reconnectionTimer.Close();
        await server.StopAsync();
        client.Close();
        await base.Stop(message);
    }

    protected override void Init()
    {
        if (Setting == null)
        {
            Setting = new AppSetting
            {
                ServerId = 1000,
                ServerType = ServerType.Router,
                TcpPort = 21000,
                // 网关配置
                GrpcPort = 22000,
                CenterUrl = "127.0.0.1",
            };
        }

        base.Init();
    }
}