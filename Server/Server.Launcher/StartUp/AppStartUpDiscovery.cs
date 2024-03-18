using Server.Launcher.PipelineFilter;
using Server.NetWork.TCPSocket;
using Server.ServerManager;
using SuperSocket.Channel;

namespace Server.Launcher.StartUp;

/// <summary>
/// 发现服务器
/// </summary>
[StartUpTag(ServerType.Discovery, 0)]
internal sealed class AppStartUpDiscovery : AppStartUpBase
{
    private IServer server;

    // readonly IMessageDecoderHandler messageDecoderHandler = new MessageActorDiscoveryDecoderHandler();
    readonly MessageActorDiscoveryEncoderHandler messageEncoderHandler = new MessageActorDiscoveryEncoderHandler();


    public override async Task EnterAsync()
    {
        try
        {
            LogHelper.Info($"开始启动服务器{ServerType}");

            NamingServiceManager.Instance.AddSelf(Setting);

            LogHelper.Info($"启动服务器{ServerType} 开始!");

            // UDP server port
            /*int port = Setting.TcpPort;
            if (port <= 0)
            {
                // 默认缺省端口
                var ports = await PortHelper.ScanPorts(33300, 33399);
                if (ports.Count > 0)
                {
                    port = ports[0];
                }
            }*/

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

            LogHelper.Info($"启动服务器 {ServerType} 端口: {Setting.TcpPort} 结束!");

            await AppExitToken;
        }
        catch (Exception e)
        {
            LogHelper.Info($"服务器执行异常，e:{e}");
            LogHelper.Fatal(e);
        }

        // Stop the server
        LogHelper.Info($"退出服务器开始");
        await server.StopAsync();
        LogHelper.Info($"退出服务器成功");
    }


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
        // var response = new RespActorHeartBeat()
        // {
        //     Timestamp = TimeHelper.UnixTimeSeconds()
        // };
        // await session.SendAsync(messageEncoderHandler, response);
    }


    private ValueTask OnConnected(IAppSession appSession)
    {
        LogHelper.Info("有外部客户端网络连接到中心服务器成功" + "。链接信息：SessionID:" + appSession.SessionID + " RemoteEndPoint:" + appSession.RemoteEndPoint);
        // NamingServiceManager.Instance.Add();
        return ValueTask.CompletedTask;
    }

    private ValueTask OnDisconnected(object sender, CloseEventArgs args)
    {
        LogHelper.Info("有外部客户端从中心服务器断开。链接信息：断开原因:" + args.Reason);
        return ValueTask.CompletedTask;
    }

    public override async Task Stop(string message = "")
    {
        LogHelper.Info($"{ServerType} Server stopping...");
        await server.StopAsync();
        LogHelper.Info($"{ServerType} Server Done!");
    }

    private void ConfigureSuperSocket(ServerOptions options)
    {
        options.AddListener(new ListenOptions { Ip = IPAddress.Any.ToString(), Port = Setting.TcpPort });
    }

    protected override void Init()
    {
        if (Setting == null)
        {
            Setting = new AppSetting
            {
                ServerId = 3300,
                ServerType = ServerType.Discovery,
                TcpPort = 33300
            };
        }

        base.Init();
    }
}