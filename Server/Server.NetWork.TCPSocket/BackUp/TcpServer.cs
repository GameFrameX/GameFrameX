/*using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Server.Setting;
using Server.Utility;

namespace Server.NetWork.TCPSocket
{
    /// <summary>
    /// TCP server
    /// </summary>
    public static class TcpServer
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static WebApplication? App { get; set; }
        public static IMessageHelper? MessageHelper { get; private set; }
        public static AppSetting AppSetting { get; private set; }

        public static Task Start(int port, AppSetting appSetting, IMessageHelper messageHelper, Action<ListenOptions>? configure = null)
        {
            Guard.NotNull(messageHelper, nameof(messageHelper));
            MessageHelper = messageHelper;
            Guard.NotNull(appSetting, nameof(appSetting));
            AppSetting = appSetting;
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options =>
                {
                    if (configure == null)
                    {
                        options.ListenAnyIP(port, listenOptions => { listenOptions.UseConnectionHandler<TcpConnectionHandler>(); });
                    }
                    else
                    {
                        options.ListenAnyIP(port, configure);
                    }
                })
                .ConfigureLogging(logging => { logging.SetMinimumLevel(LogLevel.Debug); })
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
}*/