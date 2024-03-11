using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using Server.Log;

namespace Server.NetWork.HTTP
{
    public static class HttpServer
    {
        private static WebApplication App { get; set; }

        public const string GameApiPath = "/game/api/";

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
                .UseSerilog();

            App = builder.Build();
            App.UseExceptionHandler((errorContext) =>
            {
                errorContext.Run(async (context) =>
                {
                    // 获取异常信息
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                    // 自定义返回Json信息；
                    await context.Response.WriteAsync(exceptionHandlerPathFeature.Error.Message);
                });
            });
            App.MapGet(GameApiPath + "{text}", context => HttpHandler.HandleRequest(context, baseHandler));
            App.MapPost(GameApiPath + "{text}", context => HttpHandler.HandleRequest(context, baseHandler));
            return App.StartAsync();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public static Task Stop()
        {
            if (App != null)
            {
                LogHelper.Info("停止http服务...");
                var task = App.StopAsync();
                App = null;
                return task;
            }

            return Task.CompletedTask;
        }
    }
}