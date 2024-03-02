using System.Net;
using NLog;
using Server.NetWork.UDPSocket;
using Server.Setting;

namespace Server.Launcher.StartUp;

public class AppStartUpDiscovery
{
    static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static async Task Enter(ServerType serverType = ServerType.Discovery)
    {
        try
        {
            // LogManager.Configuration = new XmlLoggingConfiguration("Configs/NLog.config");
            LogManager.AutoShutdown = false;
            GlobalSettings.Load<AppSetting>($"configs/discovery_config.{serverType.ToString()}.json", serverType);

            Log.Info("开始启动...");

            // NamingService.Instance.AddSelf();

            Console.WriteLine("启动服务器 开始!");

            // UDP server port
            int port = 3333;
            // if (args.Length > 0)
            //     port = int.Parse(args[0]);

            Console.WriteLine($"UDP server port: {port}");

            Console.WriteLine();

            // Create a new UDP echo server
            var server = new EchoUdpServer(IPAddress.Any, port);

            // Start the server
            Console.Write("Server starting...");
            server.Start();
            Console.WriteLine("Done!");

            Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (;;)
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                // Restart the server
                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    server.Restart();
                    Console.WriteLine("Done!");
                }
            }

            // Stop the server
            Console.Write("Server stopping...");
            server.Stop();
            Console.WriteLine("Done!");

            GlobalSettings.IsAppRunning = true;

            Log.Info("启动完成...");
            await GlobalSettings.Instance.AppExitToken;
        }
        catch (Exception e)
        {
            Console.WriteLine($"服务器执行异常，e:{e}");
            Log.Fatal(e);
        }

        Console.WriteLine($"退出服务器开始");
        // await RpcServer.Stop();
        Console.WriteLine($"退出服务器成功");
    }
}