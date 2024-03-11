using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Log;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Server.NetWork.RPCSocket
{
    public class RpcServer
    {
        public static IHost host { get; set; }

        public static Task Start(int rpcPort)
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options => { options.ListenAnyIP(rpcPort, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; }); })
                        .UseStartup<RpcStartup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    // logging.SetMinimumLevel(LogLevel.Warning).AddProvider(new LoggerProvider());
                });
            host = builder.Build();
            return host.StartAsync();
        }

        public static Task Stop()
        {
            LogHelper.Info("停止rpc服务...");
            return host.StopAsync();
        }
    }
}