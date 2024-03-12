using Server.EntryUtility;
using Server.Launcher.Message;
using Server.NetWork.TCPSocket;
using Server.NetWork.UDPSocket;
using Server.ServerManager;
using Server.Utility;

namespace Server.Launcher.StartUp;

/// <summary>
/// 发现服务器
/// </summary>
[StartUpTag(ServerType.Discovery, 0)]
internal sealed class AppStartUpDiscovery : AppStartUpBase
{
    private TcpServerMessage server;
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
            int port = Setting.TcpPort;
            if (port <= 0)
            {
                // 默认缺省端口
                var ports = await PortHelper.ScanPorts(33300, 33399);
                if (ports.Count > 0)
                {
                    port = ports[0];
                }
            }

            server = new TcpServerMessage(IPAddress.Any, port);
            server.MessageDecoderHandler = new MessageDecoderHandler();
            server.Start();

            LogHelper.Info($"服务器端口: {port}");

            // Create a new UDP echo server
            server = new TcpServerMessage(IPAddress.Any, Setting.TcpPort);
            server.MessageDecoderHandler = new MessageDecoderHandler();
            // Start the server
            server.Start();
            LogHelper.Info($"启动服务器 {ServerType} 结束!");

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

    public override void Stop(string message = "")
    {
        Console.Write("Server stopping...");
        server.Stop();
        LogHelper.Info("Done!");
    }
}