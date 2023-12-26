using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Server.NetWork.TCPSocket
{
    /// <summary>
    /// TCP server
    /// </summary>
    public static class TcpServer
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static WebApplication App { get; set; }
        public static IMessageHelper MessageHelper { get; private set; }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="port"></param>
        /// <param name="messageHelper"></param>
        public static Task Start(int port, IMessageHelper messageHelper)
        {
            MessageHelper = messageHelper;
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options => { options.ListenAnyIP(port, listenOptions => { listenOptions.UseConnectionHandler<TcpConnectionHandler>(); }); })
                .ConfigureLogging(logging => { logging.SetMinimumLevel(LogLevel.Error); })
                .UseNLog();

            var app = builder.Build();
            return app.StartAsync();
        }

        public static Task Start(int port, IMessageHelper messageHelper, Action<ListenOptions> configure)
        {
            MessageHelper = messageHelper;
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options => { options.ListenAnyIP(port, configure); })
                .ConfigureLogging(logging => { logging.SetMinimumLevel(LogLevel.Error); })
                .UseNLog();

            App = builder.Build();
            Log.Info("启动tcp服务...");
            return App.StartAsync();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public static Task Stop()
        {
            if (App != null)
            {
                Log.Info("停止Tcp服务...");
                var task = App.StopAsync();
                App = null;
                return task;
            }

            return Task.CompletedTask;
        }
    }
}