/*
using System.Net.Sockets;
using Server.Launcher.StartUp;
using Server.NetWork.TCPSocket.Base;
using Server.NetWork.UDPSocket;

/// <summary>
/// 路由服务器
/// </summary>
// [StartUpTag(ServerType.Router, 100)]
internal sealed class AppStartUpRouter : AppStartUpBase
{
    static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();
    private TcpRouterServer server;
    private EchoUdpClient client;

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
            client = new EchoUdpClient(address, port);
            // Connect the client
            Console.Write("开始链接到中心服务器 ...");
            client.Connect();
            Console.WriteLine("Done!");

            server = new TcpRouterServer(IPAddress.Any, port);
            server.Start();
            // var tcpSocketMessageHelper = new TcpSocketMessageHelper(HotfixMgr.GetTcpHandler, HotfixMgr.GetMsgTypeById, HotfixMgr.GetMsgIdByType);
            // await TcpServer.Start(Setting.TcpPort, Setting, tcpSocketMessageHelper, builder => { builder.UseConnectionHandler<AppTcpConnectionHandler>(); });

            await AppExitToken;
            Console.Write("全部断开...");
            server.Stop();
            client.Disconnect();
            Console.WriteLine("Done!");
        }
        catch (Exception e)
        {
        }
    }

    public override void Stop(string message)
    {
        base.Stop(message);
        server.Stop();
        client.Disconnect();
    }
}
*/
