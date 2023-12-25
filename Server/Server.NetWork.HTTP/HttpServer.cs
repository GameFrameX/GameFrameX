using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Server.NetWork.HTTP
{
    public static class HttpServer
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static WebApplication App { get; set; }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="httpPort">HTTP端口</param>
        /// <param name="httpsPort">HTTPS端口</param>
        /// <param name="baseHandler">根据命令Id获得处理器</param>
        public static Task Start(int httpPort, int httpsPort, Func<string, BaseHttpHandler> baseHandler)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options =>
                {
                    // HTTP 
                    if (httpPort > 0)
                    {
                        options.ListenAnyIP(httpPort);
                    }

                    // HTTPS
                    if (httpsPort > 0)
                    {
                        options.ListenAnyIP(httpsPort, listenOptions => { listenOptions.UseHttps(); });
                    }
                })
                .ConfigureLogging(logging => { logging.SetMinimumLevel(LogLevel.Debug); })
                .UseNLog();

            App = builder.Build();
            App.MapGet("/game/{text}", context => HttpHandler.HandleRequest(context, baseHandler));
            App.MapPost("/game/{text}", context => HttpHandler.HandleRequest(context, baseHandler));
            return App.StartAsync();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public static Task Stop()
        {
            if (App != null)
            {
                Log.Info("停止http服务...");
                var task = App.StopAsync();
                App = null;
                return task;
            }

            return Task.CompletedTask;
        }
    }
}