using Server.Launcher.PipelineFilter;
using Server.NetWork.TCPSocket;
using SuperSocket.Channel;
using ErrorEventArgs = SuperSocket.ClientEngine.ErrorEventArgs;

/// <summary>
/// 网关服务器
/// </summary>
[StartUpTag(ServerType.Gateway)]
internal sealed class AppStartUpGateway : AppStartUpBase
{
    private AsyncTcpSession client;
    IMessageEncoderHandler messageEncoderHandler = new MessageActorGatewayEncoderHandler();
    IMessageDecoderHandler messageDecoderHandler = new MessageActorGatewayDecoderHandler();
    ReqActorHeartBeat reqHeartBeat = new ReqActorHeartBeat();

    public override async Task EnterAsync()
    {
        try
        {
            LogHelper.Info($"启动服务器{Setting.ServerType} 开始! address: {Setting.LocalIp}  port: {Setting.TcpPort}");
            await StartServer();
            client = new AsyncTcpSession();
            client.Connected += ClientOnConnected;
            client.Closed += ClientOnClosed;
            client.DataReceived += ClientOnDataReceived;
            client.Error += ClientOnError;

            LogHelper.Info("开始链接到中心服务器 ...");
            client.Connect(new IPEndPoint(IPAddress.Parse(Setting.CenterUrl), Setting.GrpcPort));
            LogHelper.Info("链接完成!");
            TimeSpan delay = TimeSpan.FromSeconds(5);
            await Task.Delay(delay);

            await AppExitToken;
            Console.Write("全部断开...");
            LogHelper.Info("Done!");
        }
        catch (Exception e)
        {
            await Stop(e.Message);
            AppExitSource.TrySetException(e);
            LogHelper.Info(e);
        }
    }


    #region Server

    IServer server;

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


    // readonly MessageActorDiscoveryEncoderHandler messageEncoderHandler = new MessageActorDiscoveryEncoderHandler();

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
        // await session.SendAsync(messageEncoderHandler, response);
    }


    private ValueTask OnConnected(IAppSession appSession)
    {
        LogHelper.Info("有路由客户端网络连接成功！。链接信息：SessionID:" + appSession.SessionID + " RemoteEndPoint:" + appSession.RemoteEndPoint);
        // NamingServiceManager.Instance.Add();
        return ValueTask.CompletedTask;
    }

    private ValueTask OnDisconnected(object sender, CloseEventArgs args)
    {
        LogHelper.Info("有路由客户端网络断开连接成功！。断开信息：" + args.Reason);
        return ValueTask.CompletedTask;
    }

    private void ConfigureSuperSocket(ServerOptions options)
    {
        options.AddListener(new ListenOptions { Ip = IPAddress.Any.ToString(), Port = Setting.TcpPort });
    }

    #endregion

    #region Client

    protected override void HeartBeatTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        //心跳包
        if (client.IsConnected)
        {
            reqHeartBeat.Timestamp = TimeHelper.UnixTimeSeconds();
            reqHeartBeat.UniqueId = UtilityIdGenerator.GetNextUniqueId();
            SendMessage(reqHeartBeat);
        }
    }

    protected override void ReconnectionTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        ConnectToDiscovery();
    }

    private void ConnectToDiscovery()
    {
        client.Connect(new IPEndPoint(IPAddress.Parse(Setting.CenterUrl), Setting.GrpcPort));
    }

    private void ClientOnDataReceived(object sender, DataEventArgs e)
    {
        Span<byte> span = e.Data;

        var message = span.Slice(e.Offset, e.Length);
        var package = messageDecoderHandler.Handler(message);
        if (Setting.IsDebug && Setting.IsDebugReceive)
        {
            LogHelper.Debug($"---收到消息 ==>消息类型:{package.GetType()} 消息内容:{package}");
        }
    }

    private void ClientOnConnected(object sender, EventArgs e)
    {
        HeartBeatTimer.Start();
        ReconnectionTimer.Stop();
    }

    private void ClientOnClosed(object sender, EventArgs eventArgs)
    {
        LogHelper.Info("网络连接断开");
        HeartBeatTimer.Stop();
        ReconnectionTimer.Start();
        ConnectToDiscovery();
    }

    private void ClientOnError(object sender, ErrorEventArgs e)
    {
        LogHelper.Info("链接到中心服务器失败");
        // 开启重连
        HeartBeatTimer.Stop();
        ReconnectionTimer.Start();
        ConnectToDiscovery();
    }

    private void SendMessage(IMessage message)
    {
        var span = messageEncoderHandler.Handler(message);
        if (Setting.IsDebug && Setting.IsDebugSend)
        {
            LogHelper.Debug($"---发送消息ID:[{ProtoMessageIdHandler.GetReqMessageIdByType(message.GetType())}] ==>消息类型:{message.GetType()} 消息内容:{message}");
        }

        client.TrySend(span);
        ArrayPool<byte>.Shared.Return(span);
    }

    #endregion


    protected override void Init()
    {
        if (Setting == null)
        {
            Setting = new AppSetting
            {
                ServerId = 2000,
                ServerType = ServerType.Gateway,
                TcpPort = 22000,
                GrpcPort = 33300,
                CenterUrl = "127.0.0.1",
            };
        }

        base.Init();
    }
}