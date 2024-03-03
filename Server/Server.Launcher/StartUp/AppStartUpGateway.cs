using Server.Launcher.StartUp;
using Server.NetWork.UDPSocket;

/// <summary>
/// 网关服务器
/// </summary>
[StartUpTag(ServerType.Gateway)]
internal sealed class AppStartUpGateway : AppStartUpBase
{
    static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();

    public override async Task EnterAsync()
    {
        try
        {
            Log.Info($"开始启动服务器{ServerType}");
            // UDP server address
            string address = Setting.CenterUrl;
            // UDP server port
            int port = Setting.GrpcPort;
            Console.WriteLine($"启动服务器{ServerType} 开始! address: {address}  port: {port}");
            // Create a new TCP chat client
            var client = new EchoUdpClient(address, port);
            // Connect the client
            Console.Write("开始链接到中心服务器 ...");
            client.Connect();
            Console.WriteLine("Done!");
            await AppExitToken;
            Console.Write("全部断开...");
            Console.WriteLine("Done!");
        }
        catch (Exception e)
        {
        }
    }
}