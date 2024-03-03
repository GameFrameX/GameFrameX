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
    static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private TcpServerMessage server;

    public override async Task EnterAsync()
    {
        try
        {
            LogManager.AutoShutdown = false;
            Log.Info($"开始启动服务器{ServerType}");

            NamingServiceManager.Instance.AddSelf(Setting);

            Console.WriteLine($"启动服务器{ServerType} 开始!");

            // UDP server port

            Console.WriteLine($"server port: {Setting.TcpPort}");

            // Create a new UDP echo server
            server = new TcpServerMessage(IPAddress.Any, Setting.TcpPort);
            server.MessageDecoderHandler = new MessageDecoderHandler();
            // Start the server
            server.Start();
            Console.WriteLine($"启动服务器 {ServerType} 结束!");

            await AppExitToken;

            GlobalSettings.IsAppRunning = true;

            Log.Info("启动完成...");
            // await GlobalSettings.Instance.AppExitToken;
        }
        catch (Exception e)
        {
            Console.WriteLine($"服务器执行异常，e:{e}");
            Log.Fatal(e);
        }

        // Stop the server
        Console.WriteLine($"退出服务器开始");
        server.Stop();
        Console.WriteLine($"退出服务器成功");
    }

    public override void Stop(string message = "")
    {
        Console.Write("Server stopping...");
        server.Stop();
        Console.WriteLine("Done!");
    }
}