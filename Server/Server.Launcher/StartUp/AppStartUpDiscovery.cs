using System.Text;
using Server.Launcher.Message;
using Server.NetWork;
using Server.NetWork.Messages;
using Server.NetWork.TCPSocket;
using Server.ServerManager;

namespace Server.Launcher.StartUp;

/// <summary>
/// 发现服务器
/// </summary>
[StartUpTag(ServerType.Discovery, 0)]
internal sealed class AppStartUpDiscovery : AppStartUpBase
{
    private TcpServerMessage server;
    readonly IMessageDecoderHandler messageDecoderHandler = new MessageDecoderHandler();
    readonly IMessageEncoderHandler messageEncoderHandler = new MessageEncoderHandler();

    public override void Init()
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

            server = new TcpServerMessage(IPAddress.Any, Setting.TcpPort);
            server.NetWorkChannelHelper = new NetWorkChannelHelper();
            server.NetWorkChannelHelper.OnError = OnError;
            server.NetWorkChannelHelper.OnSendMessage = OnSendMessage;
            server.NetWorkChannelHelper.OnConnected = OnConnected;
            server.NetWorkChannelHelper.OnDisconnected = OnDisconnected;
            server.NetWorkChannelHelper.OnReceiveMessage = OnReceiveMessage;
            server.Start();
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
        server.Stop();
        LogHelper.Info($"退出服务器成功");
    }

    private byte[] OnSendMessage(IMessage arg)
    {
        return this.messageEncoderHandler.Handler(arg);
    }

    private void OnReceiveMessage(ISession session, byte[] buffer, long offset, long size)
    {
        var message = buffer.AsSpan((int)offset, (int)size);
        var messageObject = this.messageDecoderHandler.Handler(message);
        if (messageObject is MessageObject msg)
        {
            var messageId = msg.MessageId;
            if (Setting.IsDebug && Setting.IsDebugReceive)
            {
                LogHelper.Debug($"---收到消息ID:[{messageId}] ==>消息类型:{msg.GetType()} 消息内容:{messageObject}");
            }
        }

        var bytes = Encoding.UTF8.GetBytes(messageObject.ToString());
        session.Send(bytes);
    }

    private void OnDisconnected()
    {
        LogHelper.Info("网络连接断开");
    }

    private void OnConnected()
    {
        LogHelper.Info("网络连接成功");
    }

    private void OnError(string obj)
    {
        LogHelper.Info("网络连接错误：" + obj);
    }

    public override void Stop(string message = "")
    {
        LogHelper.Info("Server stopping...");
        server.Stop();
        LogHelper.Info("Done!");
    }
}