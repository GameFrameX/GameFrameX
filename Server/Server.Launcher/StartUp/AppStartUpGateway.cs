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
            LogHelper.Info($"启动服务器{ServerType} 开始! address: {Setting.CenterUrl}  port: {Setting.GrpcPort}");

            client = new AsyncTcpSession();
            client.Connected += ClientOnConnected;
            client.Closed += ClientOnClosed;
            client.DataReceived += ClientOnDataReceived;


            LogHelper.Info("开始链接到中心服务器 ...");
            client.Connect(new IPEndPoint(IPAddress.Parse(Setting.CenterUrl), Setting.GrpcPort));
            LogHelper.Info("链接完成!");
            TimeSpan delay = TimeSpan.FromSeconds(5);
            await Task.Delay(delay);

            var timer = new System.Timers.Timer(5000);
            timer.Elapsed += TimerOnElapsed;
            timer.Enabled = true;
            timer.Start();

            await AppExitToken;
            Console.Write("全部断开...");
            LogHelper.Info("Done!");
        }
        catch (Exception e)
        {
            Stop(e.Message);
            AppExitSource.TrySetException(e);
            LogHelper.Info(e);
        }
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        //心跳包
        if (client.IsConnected)
        {
            reqHeartBeat.Timestamp = TimeHelper.UnixTimeSeconds();
            reqHeartBeat.UniqueId = UtilityIdGenerator.GetNextUniqueId();
            SendMessage(reqHeartBeat);
        } //断线重连
        else if (!client.IsConnected)
        {
            client.Connect(new IPEndPoint(IPAddress.Parse(Setting.CenterUrl), Setting.GrpcPort));
        }
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
    }

    private void ClientOnClosed(object sender, EventArgs eventArgs)
    {
        LogHelper.Info("网络连接断开");
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